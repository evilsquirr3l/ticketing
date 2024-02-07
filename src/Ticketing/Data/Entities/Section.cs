namespace Ticketing.Data.Entities;

public class Section : BaseEntity
{
    public required string Name { get; set; }

    public Guid ManifestId { get; set; }
    
    public virtual Manifest Manifest { get; set; } = null!;

    public virtual ICollection<Row> Rows { get; set; } = null!;
    
    public virtual ICollection<Offer> Offers { get; set; } = null!;
}