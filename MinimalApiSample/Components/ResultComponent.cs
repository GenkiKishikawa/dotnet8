using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MinimalApiSample.Models;

namespace MinimalApiSample.Components
{
    public interface IResultComponent
    {
        // <summary>
        // ランディング遷移URLにある可変部分(ログインユーザ等)を置き換えするオブジェクト
        // </summary>
        // <value>置換オブジェクト</value>
        UriReplacer UriReplacer { set; get; }

        // <summary>
        // ResultModelオブジェクトを返却する
        // </summary>
        // <returns>ResultModelオブジェクト</returns>
        ResultModel GetResult();

        // <summary>
        // ステータスセッター
        // </summary>
        // <param name="status">ステータス</param>
        // <returns>本オブジェクト</returns>
        IResultComponent SetStatus(int status);

        // <summary>
        // メッセージIDセッター
        // </summary>
        // <param name="messageId">メッセージID</param>
        // <returns>本オブジェクト</returns>
        IResultComponent SetMessageId(string messageId);

        // <summary>
        // メッセージセッター
        // </summary>
        // <param name="message">メッセージ</param>
        // <returns>本オブジェクト</returns>
        IResultComponent SetMessage(string message);

        // <summary>
        // 内部で保持している照査項目リストに項目リストを追加する。
        // </summary>
        // <param name="shosas">照査項目リスト</param>
        // <returns>本オブジェクト</returns>
        IResultComponent AddShosas(IList<IResultItemModel> shosas);

        // <summary>
        // エラー照査項目リストを作成する
        // </summary>
        // <param name="items">照査項目リスト</param>
        // <returns>エラー照査項目リスト</returns>
        List<IResultItemModel> CreateErrorShosas(ItemModel[] items, bool isTimeout);
    }

    public class ResultComponent : IResultComponent
    {
        private ResultModel _resultModel;

        public UriReplacer UriReplacer { get; set; }

        public ResultComponent()
        {
            _resultModel = new ResultModel();
        }

        // <inheritdoc />
        public ResultModel GetResult()
        {
            this._resultModel.GeneratedTime = DateTime.Now;
            return _resultModel;
        }

        // <inheritdoc />
        public IResultComponent SetStatus(int status)
        {
            _resultModel.Status = status;
            return this;
        }

        // <inheritdoc />
        public IResultComponent SetMessageId(string messageId)
        {
            _resultModel.MessageId = messageId;
            return this;
        }

        // <inheritdoc />
        public IResultComponent SetMessage(string message)
        {
            _resultModel.Message = message;
            return this;
        }

        // <inheritdoc />
        public IResultComponent AddShosas(List<IResultItemModel> shosas)
        {
            shosas = this.ReplaceUri(shosas);

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
        public List<IResultItemModel> CreateErrorShosas(ItemModel[] items, bool isTimeout)
        {
            var shosas = new List<IResultItemModel>();
            foreach (ItemModel item in items)
            {
                int? count = CountStatus.NotFound;
                if (item.Default != null)
                {
                    count = item.DefaultCount;
                }
                else if (isTimeout)
                {
                    count = CountStatus.NotFound;
                }
                else
                {
                    count = CountStatus.Error
                }

                var shosa = new ShosaModel
                {
                    id = item.Id,
                    count = count,
                    uri = item.Uri
                };
                shosas.Add(shosa);
            }
            return shosas;
        }
    }

    // <summary>
    // ランディング遷移URLにある可変部分(ログインユーザ等)を置き換えするオブジェクト
    // </summary>
    public class UriReplacer
    {
        public string LoginId { get; set; }

        // <summary>
        // 入力文字列の置き換え対象部分をプロパティで置換する
        // </summary>
        // <param name="input">入力文字列</param>
        // <returns>置換後文字列</returns>
        public string Replace(string input)
        {
            if (input == null)
            {
                return null;
            }
            Regex re = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);
            var output = re.Replace(input, match =>
                {
                    var pi = this.GetType().GetProperty(match.Groups[1].Value);
                    return pi.GetValue(this).ToString();
                }
            );
            return output;
        }
    }

}