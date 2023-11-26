namespace Ticketing.Data.Entities;

public class Manifest : BaseEntity
{
    public required string Map { get; set; }

    public virtual ICollection<Section> Sections { get; set; } = null!;
}