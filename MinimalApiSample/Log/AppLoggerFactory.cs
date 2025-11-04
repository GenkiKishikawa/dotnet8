using Microsoft.Extensions.Logging;

namespace MinimalApiSample.Log
{
  /// <summary>
  /// アプリ独自ロガーファクトリインターフェース
  /// </summary>
  public interface IAppLoggerFactory
  {
    /// <summary>
    /// AppLoggerオブジェクトをインスタンス化する。
    /// </summary>
    /// <param name="categoryName">カテゴリ名</param>
    /// <returns>ロガーオブジェクト</returns>
    AppLogger CreateLogger(string categoryName);
  }

  /// <summary>
  /// アプリ独自ロガーファクトリ
  /// </summary>
  public class AppLoggerFactory : IAppLoggerFactory
  {
    private readonly ILoggerFactory _loggerFactory;

    public AppLoggerFactory(ILoggerFactory loggerFactory)
    {
      this._loggerFactory = loggerFactory;
    }

    /// <summary>
    /// AppLoggerオブジェクトをインスタンス化する。
    /// </summary>
    /// <param name="categoryName">カテゴリ名</param>
    /// <returns>ロガーオブジェクト</returns>
    public AppLogger CreateLogger(string categoryName)
    {
      var logger = this._loggerFactory.CreateLogger(categoryName);
      return new AppLogger(logger);
    }
  }
}