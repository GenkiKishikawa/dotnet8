using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinimalApiSample.Models;
using MinimalApiSample.Components;
using MinimalApiSample.Log;
using MinimalApiSample.Exceptions;
using HtmlAgilityPack;
using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.RegularExpressions;
using MinimalApiSample.Extensions;

namespace MinimalApiSample.Controllers
{
  [Route("api/logistics")]
  [ApiController]
  public class LogisticsController : ControllerBase
  {
    private readonly LogisticsSettingsModel _settings;

    private readonly IGraphAPIComponent _graphApi;

    private readonly IResultComponent _result;

    private readonly IHttpClientFactory _clientFactory;

    private readonly IConfiguration _config;

    private readonly AppLogger _appLogger;

    private static readonly Regex CountRegex = new Regex(@"(\d+)件", RegexOptions.Compiled);

    public LogisticsController(
      IOptions<LogisticsSettingsModel> settings,
      IGraphAPIComponent graphApi,
      IResultComponent result,
      IHttpClientFactory clientFactory,
      IConfiguration config,
      IAppLoggerFactory appLoggerFactory
    )
    {
      _settings = settings.Value;
      _graphApi = graphApi;
      _result = result;
      _clientFactory = clientFactory;
      _config = config;
      _appLogger = appLoggerFactory.CreateLogger(typeof(LogisticsController).FullName);
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<ResultModel>> Get([FromQuery(Name = "upn")] string? upnParam = null)
    {
      try
      {
        var upn = string.IsNullOrEmpty(upnParam) 
          ? this._graphApi.GetUpn(HttpContext.Request)
          : upnParam;
        var userId = await this._graphApi.GetUserIdAsync(upn);

        this._appLogger.properties = new AppLoggerProperties
        {
          UserId = userId,
          upn = upn
        };

        HttpResponseMessage response = await this.GetResponseAsync(userId);
        var shosas = await this.GenerateShosaListAsync(response);

        this._result
            .AddShosas(shosas)
            .SetStatus(ApiStatus.Success)
            .SetMessage(null)
            .SetMessageId(null);

        return this._result.GetResult();
      }
      catch (AppException ex)
      {
        if (ex.messageId == "RM_API_17_020")
        {
          this._result
              .AddShosas(this._result.CreateErrorShosas(this._settings.Items, true))
              .SetStatus(ApiStatus.Error);

          return this._result.GetResult();
        }

        Request.Headers.TryGetValue("Accept-Language", out var lang);
        var resMsg = (lang == "en")
            ? this._config.GetValue<string>("MessagesEn:" + ex.messageId)
            : this._config.GetValue<string>("Messages:" + ex.messageId);

        this._result
            .AddShosas(this._result.CreateErrorShosas(this._settings.Items, false))
            .SetStatus(ApiStatus.Error)
            .SetMessage(resMsg)
            .SetMessageId(ex.messageId);

        return this._result.GetResult();
      }
    }

    private async Task<HttpResponseMessage> GetResponseAsync(string userId)
    {
      try
      {
        HttpClientHandler handler = new HttpClientHandler
        {
          UseDefaultCredentials = false,
          Credentials = new NetworkCredential(_settings.KerberosAuthId, _settings.KerberosAuthPassword),
          ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        HttpClient httpClient = new HttpClient(handler);
        httpClient.Timeout = TimeSpan.FromSeconds(_settings.Timeout);

        string url = QueryHelpers.AddQueryString(_settings.ShosaUrl_Logistics, "UserId", userId);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        HttpResponseMessage response = await httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
          var logMsg = this._config.GetValue<string>("Messages:RM_API_17_020");
          var logRes = "\r\n" + await response.ToLogStringAsync();
          this._appLogger.Write("Error", logMsg, logRes);

          throw new AppException
          {
            messageId = "RM_API_17_030"
          };
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
          var logMsg = this._config.GetValue<string>("Messages:RM_API_17_010");
          var logRes = "\r\n" + await response.ToLogStringAsync();
          this._appLogger.Write("Error", logMsg, logRes);
          throw new AppException
          {
            messageId = "RM_API_17_010"
          };
        }

        return response;
      }
      catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException)
      {
        var logMsg = this._config.GetValue<string>("Messages:RM_API_17_010");
        this._appLogger.Write("Error", ex, logMsg, "");

        throw new AppException
        {
          messageId = "RM_API_17_020"
        };
      }
    }

    private async Task<List<IResultItemModel>> GenerateShosaListAsync(HttpResponseMessage response)
    {
      var shosas = new List<IResultItemModel>();
      string content = await response.Content.ReadAsStringAsync();

      HtmlDocument doc = new HtmlDocument();
      doc.LoadHtml(content);

      HtmlNodeCollection rows = doc.DocumentNode.SelectNodes("//table//tr");

      foreach (var item in _settings.Items)
      {
        int? count = null;

        if (rows != null)
        {
          var matchedRow = rows.FirstOrDefault(row => ExtractSystemName(row) == item.Name);
          if (matchedRow != null)
          {
            count = ExtractCount(matchedRow);
          }
        }

        ShosaModel shosa = new ShosaModel
        {
          id = item.Id,
          count = count,
        };
        shosas.Add(shosa);
      }

      return shosas;
    }

    private string? ExtractSystemName(HtmlNode row)
    {
      HtmlNode td1 = row.SelectSingleNode("td[1]");
      if (td1 == null)
      {
        return null;
      }

      HtmlNode linkNode = td1.SelectSingleNode(".//a");
      string rawText = linkNode?.InnerText ?? td1.InnerText;

      string systemName = HtmlEntity.DeEntitize(rawText).Trim();

      return systemName;
    }

    private int? ExtractCount(HtmlNode row)
    {
      HtmlNode td2 = row.SelectSingleNode("td[2]");
      if (td2 == null)
      {
        return null;
      }

      Match match = CountRegex.Match(td2.InnerText);
      int? count = match.Success ? int.Parse(match.Groups[1].Value) : null;

      return count;
    }
  }
}
