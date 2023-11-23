namespace Ticketing.Data.Entities;

public class Event
{
    public Guid Id { get; set; }
    
    public required string Name { get; set; }

    public virtual ICollection<Venue> Venues { get; set; } = null!;
}
