namespace Ticketing.Data.Entities;

public class Venue : BaseEntity
{
    public required string Location { get; set; }

    public Guid EventId { get; set; }
    
    public virtual Event Event { get; set; } = null!;

    public Guid ManifestId { get; set; }
    
    public virtual Manifest Manifest { get; set; } = null!;
}