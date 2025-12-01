namespace MinimalApiSample.Models
{
  public class UnitedApiSettingsModel
  {
    public string ApiBaseUrl { get; set; }

    public double Timeout { get; set; }

    public UnitedApiAuthModel Auth { get; set; }

    public GroupIdModel[] GroupIds { get; set; }

    public string[] GroupIdArray => GroupIds.Where(g => g.Disable != true)
                                            .Select(g => g.GroupId)
                                            .ToArray();
  }

  public class UnitedApiAuthModel
  {
    public string BaseUrl { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public string Scope { get; set; }

    public string GrantType { get; set; }
  }

  public class GroupIdModel
  {
    public string GroupId { get; set; }

    public bool? Disable { get; set; }

    public UnitedApiItemModel[]? Items { get; set; }
  }
}