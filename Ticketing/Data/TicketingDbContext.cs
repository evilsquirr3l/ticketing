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

    public DbSet<Seat> Seats { get; set; } = null!;

    public DbSet<Row> Rows { get; set; } = null!;

    public DbSet<Section> Sections { get; set; } = null!;

    public DbSet<Venue> Venues { get; set; } = null!;

    public DbSet<Event> Events { get; set; } = null!;

    public DbSet<Cart> Carts { get; set; } = null!;

    public DbSet<CartItem> CartItems { get; set; } = null!;

    public DbSet<Price> Prices { get; set; } = null!;

    public DbSet<Customer> Customers { get; set; } = null!;

    public DbSet<Payment> Payments { get; set; } = null!;

    public DbSet<Offer> Offers { get; set; } = null!;

    public DbSet<Manifest> Manifests { get; set; } = null!;
}
