using Microsoft.EntityFrameworkCore;
using Ticketing.Data.Entities;

namespace Ticketing.Data;

public class TicketingDbContext : DbContext
{
    public TicketingDbContext(DbContextOptions<TicketingDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Offer>()
            .ToTable(x => x.HasCheckConstraint("offer_section_seat_check",
                "(\"SectionId\" IS NOT NULL AND \"SeatId\" IS NULL) OR (\"SectionId\" IS NULL AND \"SeatId\" IS NOT NULL)"));
    }

    public required DbSet<Seat> Seats { get; set; }
    
    public required DbSet<Row> Rows { get; set; }
    
    public required DbSet<Section> Sections { get; set; }
    
    public required DbSet<Venue> Venues { get; set; }
    
    public required DbSet<Event> Events { get; set; }

    public required DbSet<Cart> Carts { get; set; }
    
    public required DbSet<CartItem> CartItems { get; set; }

    public required DbSet<Price> Prices { get; set; }

    public required DbSet<Customer> Customers { get; set; }
    
    public required DbSet<Payment> Payments { get; set; }
    
    public required DbSet<Offer> Offers { get; set; }
    
    public required DbSet<Manifest> Manifests { get; set; }
}
