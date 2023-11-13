namespace Ticketing.Data.Entities;

public sealed class Section
{
    public Guid Id { get; set; }
    
    public required string Name { get; set; }
    
    public Guid VenueId { get; set; }
    
    public required Venue Venue { get; set; }
    
    public required ICollection<Row> Rows { get; set; }
}
