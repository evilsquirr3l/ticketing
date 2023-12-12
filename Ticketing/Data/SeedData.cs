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
            if (context.Database.ProviderName is not "Microsoft.EntityFrameworkCore.InMemory" && !await context.Venues.AnyAsync())
            {
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
                var cartItem = new CartItem  { Offer = offer, Cart = cart };
                await context.AddRangeAsync(venue, event1, manifest, section, row, seat, price, offer, payment, customer, cart, cartItem);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
