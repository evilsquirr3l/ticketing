namespace Ticketing.Data.Entities;

public sealed class Venue
{
    public Guid Id { get; set; }
    
    public required string Location { get; set; }
    
    public Guid EventId { get; set; }
    
    public required Event Event { get; set; }
    
    public required ICollection<Section> Sections { get; set; }
}
