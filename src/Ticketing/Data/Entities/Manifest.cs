namespace Ticketing.Data.Entities;

public class Manifest : BaseEntity
{
    public required string Map { get; set; }

    public Venue Venue { get; set; } = null!;

    public virtual ICollection<Section> Sections { get; set; } = null!;
}
