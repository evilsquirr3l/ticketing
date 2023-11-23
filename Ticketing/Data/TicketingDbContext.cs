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

    public required DbSet<Cart> Carts { get; set; }
    
    public required DbSet<CartItem> CartItems { get; set; }

    public required DbSet<Price> Prices { get; set; }

    public required DbSet<Customer> Customers { get; set; }
    
    public required DbSet<SeatPrice> SeatPrices { get; set; }
    
    public required DbSet<Payment> Payments { get; set; }
}
