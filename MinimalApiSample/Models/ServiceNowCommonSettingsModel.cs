namespace MinimalApiSample.Models
{
  /// <summary>
  /// ServiceNow共通API、設定モデル
  /// </summary>
  public class ServiceNowCommonSettingsModel
  {
    /// <summary>ServiceNow共通API取得用URL</summary>
    public string ShosaUrl_ServiceNowCommon { get; set; }
    /// <summary>APIMサブスクリプションキー(主キー)</summary>
    public string SubscriptionKey1 { get; set; }
    /// <summary>APIMサブスクリプションキー(2次キー)</summary>
    public string SubscriptionKey2 { get; set; }
    /// <summary>照査項目</summary>
    public ItemModel[] Items { get; set; }
  }
}
