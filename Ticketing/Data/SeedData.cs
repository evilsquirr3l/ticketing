using Microsoft.EntityFrameworkCore;
using Ticketing.Data.Entities;

namespace Ticketing.Data;

public static class MigrationManager
{
    public static async Task SeedData(this WebApplication webApp)
    {
        using var scope = webApp.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
        try
        {
            if (context.Database.ProviderName is not "Microsoft.EntityFrameworkCore.InMemory" && await context.Database.EnsureCreatedAsync())
            {
                var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();

                if (pendingMigrations.Any())
                {
                    await context.Database.MigrateAsync();
                }
                
                var venue = new Venue { Location = "New York" };
                var event1 = new Event { Name = "Event 1", Venue = venue };
                var manifest = new Manifest { Venue =  venue, Map = "maybe a byte array? maybe a json?" };
                var section = new Section { Name = "Section 1", Manifest = manifest };
                var row = new Row { Number = "Row 1", Section = section };
                var seat = new Seat { SeatNumber = "Seat 1", Row = row };
                var price = new Price { Amount = 100 };
                var offer = new Offer { Event = event1, Seat = seat, OfferType = "VIP", };
                var payment = new Payment { Amount = 100, Offer = offer, PaymentDate = DateTime.UtcNow};
                var customer = new Customer { Name = "Jon Doe", Email = "example@gmail.com" };
                var cart = new Cart { Customer = customer };
                var cartItems = new List<CartItem> { new() { Offer = offer, Cart = cart} };
                await context.Venues.AddAsync(venue);
                await context.Events.AddAsync(event1);
                await context.Manifests.AddAsync(manifest);
                await context.Sections.AddAsync(section);
                await context.Rows.AddAsync(row);
                await context.Seats.AddAsync(seat);
                await context.Prices.AddAsync(price);
                await context.Offers.AddAsync(offer);
                await context.Payments.AddAsync(payment);
                await context.Customers.AddAsync(customer);
                await context.Carts.AddAsync(cart);
                await context.CartItems.AddRangeAsync(cartItems);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
