using System.Reflection;
using System.Text;

namespace MinimalApiSample.Log
{
  public class AppLogger
  {
    private readonly ILogger _logger;
    public AppLoggerProperties properties { get; set; }
    public AppLogger(ILogger logger)
    {
      this._logger = logger;
      this.properties = new AppLoggerProperties();
    }

    /// <summary>
    /// ログ出力
    /// </summary>
    /// <param name="level">ログレベル</param>
    /// <param name="message">ログメッセージ</param>
    /// <param name="arg">可変引数</param>
    public void Write(string level, string message, params object[] args)
    {
      var fullMsg = this.CreateMessageTemplate(message);
      var fullProps = this.CreatePropertiesArray(args);

      switch (level)
      {
        case "Trace":
          this._logger.LogTrace(fullMsg, fullProps);
          break;
        case "Debug":
          this._logger.LogDebug(fullMsg, fullProps);
          break;
        case "Information":
          this._logger.LogInformation(fullMsg, fullProps);
          break;
        case "Warning":
          this._logger.LogWarning(fullMsg, fullProps);
          break;
        case "Error":
          this._logger.LogError(fullMsg, fullProps);
          break;
        case "Critical":
          this._logger.LogCritical(fullMsg, fullProps);
          break;
        default:
          throw new ArgumentException($"Not supported log level: {level}");
      }
    }

    /// <summary>
    /// ログ出力（例外付き）
    /// </summary>
    /// <param name="level">ログレベル</param>
    /// <param name="exception">例外オブジェクト</param>
    /// <param name="message">ログメッセージ</param>
    /// <param name="arg">可変引数</param>
    public void Write(string level, Exception ex, string message, params object[] args)
    {
      var fullMsg = this.CreateMessageTemplate(message);
      var fullProps = this.CreatePropertiesArray(args);

      switch (level)
      {
        case "Trace":
          this._logger.LogTrace(ex, fullMsg, fullProps);
          break;
        case "Debug":
          this._logger.LogDebug(ex, fullMsg, fullProps);
          break;
        case "Information":
          this._logger.LogInformation(ex, fullMsg, fullProps);
          break;
        case "Warning":
          this._logger.LogWarning(ex, fullMsg, fullProps);
          break;
        case "Error":
          this._logger.LogError(ex, fullMsg, fullProps);
          break;
        case "Critical":
          this._logger.LogCritical(ex, fullMsg, fullProps);
          break;
        default:
          throw new ArgumentException($"Not supported log level: {level}");
      }
    }

    /// <summary>
    /// ログメッセージテンプレートを生成する。
    /// </summary>
    /// <param name="message">ログメッセージ</param>
    /// <returns>ログメッセージテンプレート</returns>
    private string CreateMessageTemplate(string message)
    {
      if (this.properties == null)
      {
        return message;
      }

      PropertyInfo[] props = this.properties.GetType().GetProperties();
      var sb = new StringBuilder();

      foreach (PropertyInfo pi in props)
      {
        sb.Append($"{{pi.Name}}\t");
      }

      return sb.ToString() + message;
    }

    /// <summary>
    /// ログメッセージテンプレートに対応するログ項目を生成する。
    /// </summary>
    /// <param name="args">可変引数</param>
    /// <returns>ログ項目</returns>
    private object[] CreatePropertiesArray(object[] args)
    {
      if (this.properties == null)
      {
        return args;
      }

      PropertyInfo[] props = this.properties.GetType().GetProperties();
      var propValues = new List<object>();

      foreach (PropertyInfo pi in props)
      {
        propValues.Add(pi.GetValue(this.properties));
      }

      propValues.AddRange(args);

      return propValues.ToArray();
    }
  }
}