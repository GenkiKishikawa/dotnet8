namespace MinimalApiSample.Models
{
  public class UnitedApiItemModel
  {
    /// <summary>照査項目ID</summary>
    public string Id { get; set; }
    /// <summary>照査項目名</summary>
    public string Name { get; set; }
    
    public bool? Disable { get; set; }
  }
}