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

        public ResultModel GetResult()
        {
            this._resultModel.GeneratedTime = DateTime.Now;
            return _resultModel;
        }

        public IResultComponent SetStatus(int status)
        {
            _resultModel.Status = status;
            return this;
        }

        public IResultComponent SetMessageId(string messageId)
        {
            _resultModel.MessageId = messageId;
            return this;
        }

        public IResultComponent SetMessage(string message)
        {
            _resultModel.Message = message;
            return this;
        }

        public IResultComponent AddShosas(IList<IResultItemModel> shosas)
        {
            if (_resultModel.Shosas == null)
            {
                _resultModel.Shosas = new List<IResultItemModel>();
            }
            foreach (var shosa in shosas)
            {
                _resultModel.Shosas.Add(shosa);
            }
            return this;
        }

        public List<IResultItemModel> CreateErrorShosas(ItemModel[] items, bool isTimeout)
        {
            var errorShosas = new List<IResultItemModel>();
            foreach (var item in items)
            {
                if (item.IsError)
                {
                    errorShosas.Add(new ResultItemModel
                    {
                        ItemName = item.ItemName,
                        ErrorMessage = item.ErrorMessage
                    });
                }
            }
            return errorShosas;
        }
    }
}