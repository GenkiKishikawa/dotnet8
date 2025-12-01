using Microsoft.Extensions.Options;
using MinimalApiSample.Models;

namespace MinimalApiSample.Components
{
  public interface IUnitedApiResultComponent
  {
    // <summary>
    // UnitedApiResultModelオブジェクトを返却する
    // </summary>
    // <returns>UnitedApiResultModelオブジェクト</returns>
    UnitedApiResultModel GetResult();

    // <summary>
    // ステータスセッター
    // </summary>
    // <param name="status">ステータス</param>
    // <returns>本オブジェクト</returns>
    IUnitedApiResultComponent SetStatus(int status);

    // <summary>
    // メッセージIDセッター
    // </summary>
    // <param name="messageId">メッセージID</param>
    // <returns>本オブジェクト</returns>
    IUnitedApiResultComponent SetMessageId(string messageId);

    // <summary>
    // メッセージセッター
    // </summary>
    // <param name="message">メッセージ</param>
    // <returns>本オブジェクト</returns>
    IUnitedApiResultComponent SetMessage(string message);

    // <summary>
    // 内部で保持している照査項目リストに項目リストを追加する。
    // </summary>
    // <param name="shosas">照査項目リスト</param>
    // <returns>本オブジェクト</returns>
    IUnitedApiResultComponent AddShosas(List<UnitedApiShosaModel> shosas);

    // <summary>
    // エラー照査項目リストを作成する
    // </summary>
    // <returns>エラー照査項目リスト</returns>
    List<IUnitedApiResultItemModel> CreateErrorShosas();
  }

  public class UnitedApiResultComponent : IUnitedApiResultComponent
  {
    private UnitedApiResultModel _resultModel;
    private readonly UnitedApiSettingsModel _settings;
    private readonly IConfiguration _config;
    public UnitedApiResultComponent
    (
      IOptions<UnitedApiSettingsModel> settings,
      IConfiguration config
    )
    {
      _resultModel = new UnitedApiResultModel();
      _settings = settings.Value;
      _config = config;
    }

    // <inheritdoc />
    public UnitedApiResultModel GetResult()
    {
      this._resultModel.GeneratedTime = DateTime.Now;
      return _resultModel;
    }

    // <inheritdoc />
    public IUnitedApiResultComponent SetStatus(int status)
    {
      _resultModel.Status = status;
      return this;
    }

    // <inheritdoc />
    public IUnitedApiResultComponent SetMessageId(string? messageId)
    {
      _resultModel.MessageId = messageId;
      return this;
    }

    // <inheritdoc />
    public IUnitedApiResultComponent SetMessage(string? message)
    {
      _resultModel.Message = message;
      return this;
    }

    // <inheritdoc />
    public IUnitedApiResultComponent AddShosas(List<UnitedApiShosaModel> shosas)
    {
      if(this._resultModel.Data == null)
      {
        this._resultModel.Data = shosas;
      }
      else
      {
        this._resultModel.Data.AddRange(shosas);
      }

      return this;
    }

    // <inheritdoc />
    public List<IUnitedApiResultItemModel> CreateErrorShosas()
    {
      const string ErrorMessageId = "RM_API_110_010";
      var shosas = new List<IUnitedApiResultItemModel>();

      foreach (GroupIdModel group in _settings.GroupIds)
      {
        if (group.Disable == true) continue;
        if (group.Items == null) continue;
        if (string.IsNullOrWhiteSpace(group.GroupId)) continue;

        foreach (var item in group.Items)
        {
          if (item.Disable == true) continue;
          if (string.IsNullOrWhiteSpace(item.Id)) continue;
          if (string.IsNullOrWhiteSpace(item.Name)) continue;

          UnitedApiShosaModel shosa = new()
          {
            id = item.Id,
            group_id = group.GroupId,
            name = item.Name,
            status = UnitedApiApiStatus.Error,
            messageId = ErrorMessageId,
            message = this._config.GetValue<string>($"Messages:{ErrorMessageId}").Replace("{groupId}", group.GroupId),
            count = CountStatus.Error,
          };
          shosas.Add(shosa);
        }
      }
      return shosas;
    }
  }
}