
using System.Runtime.Serialization;

namespace MinimalApiSample.Models
{
  public interface IUnitedApiResultItemModel
  {
    public string id { get; set; }
    public string group_id { get; set; }
    public string name { get; set; }
    public int status { get; set; }
    public string? messageId { get; set; }
    public string? message { get; set; }
    public int? count { get; set; }
  }

  public class UnitedApiApiStatus : ApiStatus
  {
    /// <summary>パラメータ不正</summary>
    public static readonly int BadRequest = -2;
  }

  /// <summary>
  /// UnitedApi向けWebAPIの戻り値モデル
  /// </summary>
  [DataContract]
  public class UnitedApiResultModel
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

    /// <summary>照査件数リスト</summary>
    [DataMember(Name = "data")]
    public List<UnitedApiShosaModel> Data { get; set; }
  }

  /// <summary>
  /// UnitedApi照査件数モデル
  /// </summary>
  public class UnitedApiShosaModel : IUnitedApiResultItemModel
  {
    /// <summary>照査項目ID</summary>
    public string id { get; set; }

    /// <summary>グループID</summary>
    public string group_id { get; set; }

    /// <summary>照査項目名</summary>
    public string name { get; set; }

    /// <summary>ステータス</summary>
    public int status { get; set; }

    /// <summary>メッセージID</summary>
    public string? messageId { get; set; }

    /// <summary>メッセージ</summary>
    public string? message { get; set; }

    /// <summary>照査件数</summary>
    public int? count { get; set; }
  }
}