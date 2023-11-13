using Microsoft.EntityFrameworkCore;
using Ticketing.Data.Entities;

namespace Ticketing.Data;

public class TicketingDbContext : DbContext
{
    public TicketingDbContext(DbContextOptions<TicketingDbContext> options) : base(options)
    {
        
    }
    
    public required DbSet<Seat> Seats { get; set; }
    
    public required DbSet<Row> Rows { get; set; }
    
    public required DbSet<Section> Sections { get; set; }
    
    public required DbSet<Venue> Venues { get; set; }
    
    public required DbSet<Event> Events { get; set; }

    public required DbSet<Offer> Offers { get; set; }

    public required DbSet<Price> Prices { get; set; }

    public required DbSet<Customer> Customers { get; set; }
}
