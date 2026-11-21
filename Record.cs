namespace RolMaster.Models;
public sealed class Record
{
 public string Id { get; set; } = Guid.NewGuid().ToString("N");
 public string Kind { get; set; } = "campaign";
 public string Title { get; set; } = "";
 public string CampaignId { get; set; } = "";
 public string Description { get; set; } = "";
 public string System { get; set; } = "Personalizado";
 public string Data { get; set; } = "{}";
 public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
 public override string ToString() => Title;
}
