namespace Ticketing.Data.Entities;

using System.Collections.Generic;

public class Event : BaseEntity
{
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public required DateTimeOffset Date { get; set; }

    public virtual Venue Venue { get; set; } = null!;

    public virtual ICollection<Offer> Offers { get; set; } = null!;
}
