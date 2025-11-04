namespace MinimalApiSample.Models
{
  /// <summary>
  /// API共通設定モデル
  /// </summary>
  public class ApiCommonSettingsModel
  {
    /// <summary>API共通取得用URL</summary>
    public string ShosaUrl_ApiCommon { get; set; }
    /// <summary>APIMサブスクリプションキー(主キー)</summary>
    public string SubscriptionKey1 { get; set; }
    /// <summary>APIMサブスクリプションキー(2次キー)</summary>
    public string SubscriptionKey2 { get; set; }

    /// <summary>タイムアウト時間（ミリ秒）</summary>
    public int Timeout { get; set; }

    /// <summary>照査項目</summary>
    public ItemModel[] Items { get; set; }
  }
}
