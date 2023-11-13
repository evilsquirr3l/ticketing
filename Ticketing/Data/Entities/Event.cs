namespace Ticketing.Data.Entities;

public class Event
{
    public Guid Id { get; set; }
    
    public required string Name { get; set; }
    
    public virtual required ICollection<Venue> Venues { get; set; }
}
