namespace MinimalApiSample.Models
{
  public class ItemModel
  {
    /// <summary>照査項目ID</summary>
    public int Id { get; set; }
    /// <summary>照査項目名</summary>
    public string Name { get; set; }
    /// <summary>ランディングURI</summary>
    public string Uri { get; set; }
    /// <summary>ランディングURI_EN</summary>
    public string UriEn { get; set; }
    /// <summary>エラー時の既定カウント値</summary>
    public int? DefailtCount { get; set; }
  }
}