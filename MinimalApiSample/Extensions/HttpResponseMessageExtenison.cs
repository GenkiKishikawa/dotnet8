namespace MinimalApiSample.Extensions
{
  using System.Net.Http;
  using System.Threading.Tasks;

  /// <summary>
  /// HttpResponseMessageオブジェクトを拡張する。
  /// </summary>
  public static class HttpResponseMessageExtension
  {
    /// <summary>
    /// HttpResponseMessageオブジェクトをログ用の文字列に変換する。
    /// </summary>
    /// <param name="response">変換対象のHttpResponseMessageオブジェクト</param>
    /// <returns>変換後の文字列</returns>
    public static async Task<string> ToLogStringAsync(this HttpResponseMessage response)
    {
      var header = response.ToString();
      var body = await response.Content.ReadAsStringAsync();
      return $"{header}\r\n{body}";
    }
  }
}