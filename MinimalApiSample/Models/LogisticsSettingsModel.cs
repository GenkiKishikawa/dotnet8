namespace MinimalApiSample.Models
{
  /// <summary>
  /// LogisticsAPI、設定モデル
  /// </summary>
  public class LogisticsSettingsModel
  {
    /// <summary>LogisticsAPI取得用URL</summary>
    public string ShosaUrl_Logistics { get; set; }
    /// <summary>Kerberos認証ID</summary>
    public string KerberosAuthId { get; set; }
    /// <summary>Kerberos認証パスワード</summary>
    public string KerberosAuthPassword { get; set; }

    /// <summary>タイムアウト(</summary>
    public int Timeout { get; set; }

    /// <summary>照査項目</summary>
    public ItemModel[] Items { get; set; }
  }
}