using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinimalApiSample.Models;
using MinimalApiSample.Components;
using MinimalApiSample.Log;
using Newtonsoft.Json.Linq;

namespace MinimalApiSample.Controllers
{
  [Route("api/api_common")]
  [ApiController]
  public class ApiCommonController : ControllerBase
  {
    private readonly ApiCommonSettingsModel _settings;

    private readonly IGraphAPIComponent _graphApi;

    private readonly IResultComponent _result;

    private readonly IHttpClientFactory _clientFactory;

    private readonly IConfiguration _config;

    private readonly AppLogger _appLogger;

    public ApiCommonController(
      IOptions<ApiCommonSettingsModel> settings,
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
      _appLogger = appLoggerFactory.CreateLogger(typeof(ApiCommonController).FullName);
    }

    [HttpGet("get")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<ActionResult<ResultModel>> Get()
    {
      try
      {
        // プリンシパル名の取得
        var upn = _graphApi.GetUpn(HttpContext.Request);
        // ログインIDの取得
        var loginId = await _graphApi.GetLoginId(upn);

        Request.Headers.TryGetValue("Accept-Language", out var lang);
        this._appLogger.properties = new AppLoggerProperties
        {
          UserId = loginId,
          upn = upn
        };

        // APIを実行して、APIから件数を含むレスポンスを取得
        HttpResponseMessage response = await this.GetResponseAsync(loginId, lang);
        // 件数情報
        var shosas = await this. GenerateShosaListAsync(response, lang);

        return this._result.GetResult()
      }
      catch (AppException ex)
      {
        if (ex.messageId == "RM_API_16_020")
        {
          this._result
            .AddShosas(this._result.CreateErrorShosas(this._settings.Items, true))
            .SetStatus(ApiStatus.Error);
        }

        Request.Headers.TryGetValue("Accept-Language", out var lang);
        var resMsg = (lang == "en") ? this._config.GetValue<string>("MessagesEn:" + ex.messageId) : this._config.GetValue<string>("Messages:" + ex.messageId);

        this._result
            .AddShosas(this._result.CreateErrorShosas(this._settings.Items, false))
            .SetStatus(ApiStatus.Error)
            .SetMessageId(ex.messageId)

        return this._result.GetResult();
      }
    }

    private async Task<HttpResponseMessage> GetResponseAsync(string loginId, string lang)
    {
      try
      {
        var url = QueryHelpers.AddQueryString(_settings.ServiceNowCommonUrl, "employee_number", loginId);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("ContentType", "application/json; charset=utf-8" );
        request.Headers.Add("Ocp-Apim-Subscription-Key", _settings.SubscriptionKey1);

        var httpClient = _clientFactory.CreateClient("default");
        httpClient.Timeout = TimeSpan.FromMilliseconds(_settings.Timeout);

        var response = await httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
          var logMsg = (lang == "en") ? this._config.GetValue<string>("MessagesEn:LM_API_16_030") : this._config.GetValue<string>("Messages:LM_API_16_030");

        }

        // ユーザが存在しない(404)場合、生レスポンスを返却
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
          var json = await response.Content.ReadAsStringAsync();
          JObject data = JObject.Parse(json);
          if(data.ContainsKey("error") && data["status"]?.ToString() == "failure")
          {
            return response;
          }
        }

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
          var logMsg = (lang == "en") ? this._config.GetValue<string>("MessagesEn:LM_API_16_010") : this._config.GetValue<string>("Messages:LM_API_16_010");
          var logRes = "\r\n" + await response.ToLogStringAsync();
          this._appLogger.Error("Error",logMsg, logRes);
          throw new AppException
          {
            messageId = "LM_API_16_020",
          }
        }

        return response;

      }
      catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException)
      {
        var logMsg = (lang == "en") ? this._config.GetValue<string>("MessagesEn:LM_API_16_020") : this._config.GetValue<string>("Messages:LM_API_16_020");
        var logRes = "\r\n" + ex.ToLogString();
        this._appLogger.Error("Error",logMsg, logRes);
        throw new AppException
        {
          messageId = "LM_API_16_020",
        };
      }
    }

    private async Task<List<IResultItemModel>> GenerateShosaListAsync(HttpResponseMessage response, string lang)
    {
      var shosas = new List<IResultItemModel>();
      var count = 0;

      // レスポンスからJSONを取得
      var json = await response.Content.ReadAsStringAsync();
      dynamic data = JObject.Parse(json);

      if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
      {
        // ユーザが存在しない場合
        foreach (var item in this._settings.Items)
        {
          var shosa = new ShosaModel
          {
            id = item.Id,
            count = 0,
            uri = (lang == "en") ? item.UriEn : item.Uri
          };
          shosas.Add(shosa);
        }
        return shosas;
      }
      else
      {
        JObject detail = (JObject)data["result"]["detail"];
        
        foreach (var item in this._settings.Items)
          {
            int count;

            if(detail.ContainsKey(item.Name))
            {
              
              count = (int)detail.GetValue(item.Name);
            }
            else
            {
              count = 0;
            }
            var shosa = new ShosaModel
            {
              id = item.Id,
              count = count,
              uri = (lang == "en") ? item.UriEn : item.Uri
            };
            shosas.Add(shosa);
          }

          return shosas;
      }
    }
  }
}