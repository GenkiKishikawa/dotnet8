using Microsoft.Extensions.Options;
using MinimalApiSample.Log;
using MinimalApiSample.Models;
using MinimalApiSample.Exceptions;
using MinimalApiSample.Extensions;
using System.Net;
using Newtonsoft.Json;

namespace MinimalApiSample.Components
{
  /// <summary>
  /// Graph APIコンポーネントインターフェース
  /// </summary>
  public interface IGraphAPIComponent
  {
    /// <summary>
    /// HTTP RequestヘッダーからUPNを取得する
    /// </summary>
    string GetUpn(HttpRequest request);

    /// <summary>
    /// GraphAPIからユーザーIDを取得する
    /// </summary>
    /// <param name="upn">UPN</param>
    /// <return>ユーザーID</return>
    Task<string> GetUserIdAsync(string upn);
  }

  public class GraphAPIComponent : IGraphAPIComponent
  {
    private readonly GraphAPISettingModel _settings;
    private readonly AuthModel _auth;

    private readonly AppLogger _appLogger;

    private readonly IConfiguration _config;

    private readonly IHttpClientFactory _clientFactory;

    public GraphAPIComponent(
      IOptions<GraphAPISettingModel> settings,
      IOptions<AuthModel> auth,
      IHttpClientFactory clientFactory,
      IConfiguration config,
      IAppLoggerFactory appLoggerFactory
    )
    {
      this._appLogger = appLoggerFactory.CreateLogger(typeof(GraphAPIComponent).FullName);
      this._settings = settings.Value;
      this._auth = auth.Value;
      this._clientFactory = clientFactory;
      this._config = config;
    }

    public async Task<string> GetUserIdAsync(string upn)
    {
      string accessToken = await this.RequestAccessTokenAsync();
      string userId = await this.RequestUserIdAsync(accessToken, upn);

      return userId;
    }

    public string GetUpn(HttpRequest request)
    {
      if (String.IsNullOrEmpty(request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"]))
      {
        throw new ArgumentNullException("「X-MS-CLIENT-PRINCIPAL-NAME」ヘッダーが取得できませんでした。");
      }
      return request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"];
    }

    private async Task<string> RequestAccessTokenAsync()
    {
      try
      {
        var request = new HttpRequestMessage(HttpMethod.Post, this._settings.AuthAPIUrl.Replace("{tenant}", this._settings.TenantId));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
          { "client_id", this._settings.ClientId },
          { "scope", this._settings.Resource + ".default" },
          { "client_secret", this._settings.ClientSecret },
          { "grant_type", this._settings.GrantType }
        });

        var httpClient = this._clientFactory.CreateClient("default");
        httpClient.Timeout = TimeSpan.FromMilliseconds(this._settings.Timeout);
        var response = await httpClient.SendAsync(request);

        if (response.StatusCode != HttpStatusCode.OK)
        {
          var logMsg = this._config.GetValue<string>("Messages:LM_API_00_020");
          var logRes = "\r\n" + await response.ToLogStringAsync();
          this._appLogger.Write("Error", logMsg, logRes);
          throw new AppException
          {
            messageId = "LM_API_00_020",
          };
        }

        var json = await response.Content.ReadAsStringAsync();
        dynamic data = JsonConvert.DeserializeObject(json);

        return data.access_token;
      }
      catch (Exception ex) when (
        ex is HttpRequestException ||
        ex is OperationCanceledException
      )
      {
        var logMsg = this._config.GetValue<string>("Messages:LM_API_00_020");
        this._appLogger.Write("Error", ex, logMsg, "");
        throw new AppException
        {
          messageId = "LM_API_00_030",
        };
      }
    }

    /// <summary>
    /// GraphAPIからユーザーIDを取得する
    /// </summary>
    /// <param name="accessToken">アクセストークン</param>
    /// <param name="upn">UPN</param>
    /// <returns>ユーザーID</returns>
    private async Task<string> RequestUserIdAsync(string accessToken, string upn)
    {
      try
      {
        var url = $"{this._settings.BaseUrl}/users/{upn}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var httpClient = this._clientFactory.CreateClient("default");
        httpClient.Timeout = TimeSpan.FromMilliseconds(this._settings.Timeout);
        var response = await httpClient.SendAsync(request);

        if (response.StatusCode != HttpStatusCode.OK)
        {
          var logMsg = this._config.GetValue<string>("Messages:LM_API_00_020");
          var logRes = "\r\n" + await response.ToLogStringAsync();
          this._appLogger.Write("Error", logMsg, logRes);
          throw new AppException
          {
            messageId = "LM_API_00_020",
          };
        }

          string json = await response.Content.ReadAsStringAsync();
          dynamic data = JsonConvert.DeserializeObject(json);

          if (string.IsNullOrEmpty((string)data.onPremisesSamAccountName))
          {
            throw new ArgumentNullException("ユーザーIDが取得できませんでした。");
          }

          return data.onPremisesSamAccountName;
      }
      catch (Exception ex) when (
        ex is HttpRequestException ||
        ex is OperationCanceledException
      )
      {
        var logMsg = this._config.GetValue<string>("Messages:LM_API_00_020");
        this._appLogger.Write("Error", ex, logMsg, "");
        throw new AppException
        {
          messageId = "LM_API_00_030",
        };
      }
    }

    private bool IsAuthorized(string userId)
    {
      return true;
    }
  }
}