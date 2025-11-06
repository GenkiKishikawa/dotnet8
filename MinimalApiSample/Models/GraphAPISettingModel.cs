namespace MinimalApiSample.Models
{
  /// <summary>
  /// Graph API設定モデル
  /// </summary>
  public class GraphAPISettingModel
  {
    /// <summary>GraphAPIのベースURL</summary>
    public string BaseUrl { get; set; }

    /// <summary>GraphAPI認証APIのURL</summary>
    public string AuthAPIUrl { get; set; }

    /// <summary>タイムアウト</summary>
    public double Timeout { get; set; }

    /// <summary>テナントID</summary>
    public string TenantId { get; set; }

    /// <summary>クライアントシークレット</summary>
    public string ClientSecret { get; set; }

    /// <summary>リソース</summary>
    public string Resource { get; set; }

    /// <summary>GrantType</summary>
    public string GrantType { get; set; }

    /// <summary>ClientId</summary>
    public string ClientId { get; set; }
  }
}