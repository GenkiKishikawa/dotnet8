using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinimalApiSample.Models;
using MinimalApiSample.Components;
using MinimalApiSample.Log;
using MinimalApiSample.Exceptions;
using MinimalApiSample.Extensions;
using System.Net;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace MinimalApiSample.Controllers
{
  [Route("api/united_api")]
  [ApiController]
  public class UnitedApiController : ControllerBase
  {
    private readonly UnitedApiSettingsModel _settings;

    private readonly IGraphAPIComponent _graphApi;

    private readonly IParamValidatorComponent _paramValidator;

    private readonly IUnitedApiResultComponent _result;

    private readonly IHttpClientFactory _clientFactory;

    private readonly IConfiguration _config;

    private readonly AppLogger _appLogger;

    public UnitedApiController(
      IOptions<UnitedApiSettingsModel> settings,
      IGraphAPIComponent graphApi,
      IParamValidatorComponent paramValidator,
      IUnitedApiResultComponent result,
      IHttpClientFactory clientFactory,
      IConfiguration config,
      IAppLoggerFactory appLoggerFactory
    )
    {
      _settings = settings.Value;
      _graphApi = graphApi;
      _paramValidator = paramValidator;
      _result = result;
      _clientFactory = clientFactory;
      _config = config;
      _appLogger = appLoggerFactory.CreateLogger(typeof(UnitedApiController).FullName);
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<ResultModel>> Get(
      [FromQuery(Name = "upn")] string? upnParam = null,
      [FromQuery(Name = "group_id")] string? groupIdsParam = null
    )
    {
      string[] targetGroupIds = string.IsNullOrEmpty(groupIdsParam)
        ? _settings.GroupIdArray
        : groupIdsParam.Split(',').Select(id => id.Trim()).ToArray();

      try
      {
        this._paramValidator.ValidateUpn(upnParam);
        this._paramValidator.ValidateGroupIds(groupIdsParam);

        var userId = await this._graphApi.GetUserIdAsync(upnParam);
        this._appLogger.properties = new AppLoggerProperties
        {
          UserId = userId,
          upn = upnParam,
        };

        var accessToken = await this.GetAccessTokenAsync();

        ConcurrentBag<List<IResultItemModel>> shosasBag = new();

        Task[] tasks = targetGroupIds.Select(async groupId =>
        {
          try
          {
            HttpResponseMessage response = await this.GetResponseAsync(userId, groupId, accessToken);
            var shosas = await this.GenerateShosaListAsync(response, groupId);
            shosasBag.Add(shosas);
          }
          catch (AppException ex)
          {

          }
        }).ToArray();

        await Task.WhenAll(tasks);

        return this._result.GetResult();
      }
      catch (InvalidParamAppException ex)
      {
        
      }
      catch (AppException ex)
      {
        var resMsg = this._config.GetValue<string>("Messages:" + ex.messageId);

        this._result
            .AddShosas(this._result.CreateErrorShosas(this._settings.Items, false))
            .SetStatus(ApiStatus.Error)
            .SetMessage(resMsg)
            .SetMessageId(ex.messageId);
        return this._result.GetResult();
      }
    }

    private async Task<string> GetAccessTokenAsync()
    {
      try
      {
        var request = new HttpRequestMessage(HttpMethod.Post, this._settings.Auth.BaseUrl);
        request.Headers.Add("ContentType", "application/x-www-form-urlencoded; charset=UTF-8");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
          { "client_id", this._settings.Auth.ClientId },
          { "client_secret", this._settings.Auth.ClientSecret },
          { "scope", this._settings.Auth.Scope },
          { "grant_type", this._settings.Auth.GrantType },
        });

        var httpClient = this._clientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromMilliseconds(this._settings.Timeout);

        var response = await httpClient.SendAsync(request);

        if (response.StatusCode != HttpStatusCode.OK)
        {
          var logMsg = this._config.GetValue<string>("Messages:LM_API_100_020");
          var logRes = "\r\n" + await response.ToLogStringAsync();
          throw new AppException
          {
            messageId = "RM_API_100_020",
          };
        }
        var json = await response.Content.ReadAsStringAsync();
        dynamic data = JsonConvert.DeserializeObject(json);

        return data.access_token;
      }
      catch (Exception ex) when (
        ex is AppException ||
        ex is OperationCanceledException
      )
      {
        var logMsg = this._config.GetValue<string>("Messages:LM_API_100_020");
        this._appLogger.Write("Error", ex, logMsg, "");
        throw new AppException
        {
          messageId = "LM_API_100_020",
        };
      }
    }

    private async Task<HttpResponseMessage> GetResponseAsync(string userId, string groupId, string accessToken)
    {
      try
      {
        var url = QueryHelpers.AddQueryString(this._settings.ApiBaseUrl + groupId, "upn", userId);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", "Bearer " + accessToken);

        var httpClient = this._clientFactory.CreateClient("default");
        httpClient.Timeout = TimeSpan.FromMilliseconds(this._settings.Timeout);

        var response = await httpClient.SendAsync(request);

        return response;
      }
      catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException)
      {
        
      }
    }

    private async Task<List<UnitedApiShosaModel>> GenerateShosaListAsync(HttpResponseMessage response, string groupId)
    {
      var shosas = new List<UnitedApiShosaModel>();

      var content = await response.Content.ReadAsStringAsync();
      ResultModel json = JsonConvert.DeserializeObject<ResultModel>(content);

      GroupIdModel group = this._settings.GroupIds.FirstOrDefault(g => g.GroupId == groupId && g.Disable != true);

      foreach (UnitedApiItemModel item in group.Items)
      {
        IResultItemModel matched = json.Data.FirstOrDefault(d => d.id == item.Id);

        if (matched == null) continue;
 
        UnitedApiShosaModel shosa = new()
        {
          id = item.Id,
          group_id = groupId,
          name = item.Name,
          status = json.Status,
          message = json.Message,
          messageId = json.MessageId,
          count = matched.count,
        };

        shosas.Add(shosa);
      }

      return shosas;
    }
  }
}