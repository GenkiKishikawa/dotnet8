using MinimalApiSample.Log;
using MinimalApiSample.Models;
using MinimalApiSample.Exceptions;
using Microsoft.Extensions.Options;

namespace MinimalApiSample.Components
{
  public interface IParamValidatorComponent
  {
    /// <summary>
    /// UPNのバリデーションを行う
    /// </summary>
    /// <param name="upnParam">upnパラメータ</param>
    void ValidateUpn(string upnParam);

    /// <summary>
    /// グループIDのバリデーションを行う
    /// </summary>
    /// <param name="groupIdsParam">group_idパラメータ</param>
    void ValidateGroupIds(string groupIdsParam);
  }

  public class ParamValidatorComponent : IParamValidatorComponent
  {
    private readonly UnitedApiSettingsModel _unitedApiSettings;

    private readonly AppLogger _appLogger;

    private readonly IConfiguration _config;

    public ParamValidatorComponent(
      IOptions<UnitedApiSettingsModel> settings,
      IConfiguration config,
      IAppLoggerFactory appLoggerFactory
    )
    {
      _unitedApiSettings = settings.Value;
      _config = config;
      _appLogger = appLoggerFactory.CreateLogger(typeof(ParamValidatorComponent).FullName);
    }
    
    public void ValidateUpn(string upnParam)
    {
      if (string.IsNullOrWhiteSpace(upnParam))
      {
        var logMsg = this._config.GetValue<string>("Messages:LM_API_100_010");
        this._appLogger.Write("Error", logMsg);
        throw new InvalidParamAppException
        {
          messageId = "RM_API_100_010",
        };
      }
    }

    public void ValidateGroupIds(string groupIdsParam)
    {
      if (string.IsNullOrWhiteSpace(groupIdsParam))
      {
        return;
      }
      else
      {
        string[] groupIdArrayParam = groupIdsParam.Split(',').Select(s => s.Trim()).ToArray();
        foreach (var groupIdParam in groupIdArrayParam)
        {
          if (!_unitedApiSettings.GroupIdArray.Contains(groupIdParam))
          {
            var logMsg = this._config.GetValue<string>("Messages:LM_API_100_011");
            this._appLogger.Write("Error", logMsg);
            throw new InvalidParamAppException
            {
              messageId = "RM_API_100_020",
              invalidParam = groupIdParam
            };
          }
        }
      }
    }
  }
}