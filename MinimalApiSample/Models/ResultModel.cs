using System.Runtime.Serialization;

namespace MinimalApiSample.Models
{
  public interface IResultItemModel
  {
    string id { get; set; }
    int? count { get; set; }
    string uri { get; set; }
  }

  public class ApiStatus
  {
    /// <summary>疎通成功</summary>
    public static readonly int Success = 0;
    /// <summary>疎通失敗</summary>
    public static readonly int Error = -1;
  }

  public class CountStatus
  {
    public static readonly int? NotFound = null;
    public static readonly int? Hidden = -1;
    public static readonly int? Error = -2;
  }

  /// <summary>
  /// APIの戻り値モデル
  /// </summary>
  [DataContract]
  public class ResultModel
  {
    /// <summary>ステータス</summary>
    [DataMember(Name = "status")]
    public int Status { get; set; }

    /// <summary>メッセージID</summary>
    [DataMember(Name = "messageId")]
    public string? MessageId { get; set; }

    /// <summary>メッセージ</summary>
    [DataMember(Name = "message")]
    public string? Message { get; set; }

    [DataMember(Name = "generatedTime")]
    public DateTime GeneratedTime { get; set; }

    /// <summary>照査件数</summary>
    [DataMember(Name = "data")]
    public List<IResultItemModel> Data { get; set; }
  }

  /// <summary>
  /// 照査件数モデル
  /// </summary>
  public class ShosaModel : IResultItemModel
  {
    /// <summary>照査項目ID</summary>
    public string id { get; set; }

    /// <summary>照査件数</summary>
    public int? count { get; set; }

    /// <summary>ランディングURI</summary>
    public string uri { get; set; }
  }
}