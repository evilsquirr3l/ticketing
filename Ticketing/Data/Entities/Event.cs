namespace Ticketing.Data.Entities;

public sealed class Event
{
    public Guid Id { get; set; }
    
    public required string Name { get; set; }
    
    public required ICollection<Venue> Venues { get; set; }
}
