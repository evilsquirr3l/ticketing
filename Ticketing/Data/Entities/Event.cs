namespace Ticketing.Data.Entities;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Event : BaseEntity
{
    public required string Name { get; set; }

    public virtual Venue Venue { get; set; } = null!;

    public virtual ICollection<Offer> Offers { get; set; } = null!;
}
