using Ticketing.Data.Entities;

namespace UnitTests;

public class FakeItemsFactory
{
    public static Cart GetCartWithItems(Guid cartId, Guid offerId, Guid eventId, bool isSeatReserved)
    {
        var @event = new Event
        {
            Id = eventId,
            Name = "Test Event2",
            Date = DateTimeOffset.UtcNow,
            Description = "Test Description2"
        };

        var row = new Row
        {
            Number = "Test Row Number2",
            Section = new Section
            {
                Name = "Test Section Name2",
                Manifest = new Manifest
                {
                    Map = "test map",
                    Venue = new Venue
                    {
                        Event = @event,
                        Location = "test location"
                    }
                }
            }
        };

        var cart = new Cart
        {
            Id = cartId,
            Customer = new Customer
            {
                Email = "test@gmail.com",
                Name = "Test Customer"
            },
            CartItems = new List<CartItem>
            {
                new()
                {
                    Offer = new Offer
                    {
                        Id = offerId,
                        Event = @event,
                        Price = new Price
                        {
                            Amount = 100m
                        },
                        OfferType = "Test Offer Type1",
                        Seat = new Seat
                        {
                            SeatNumber = "Test Seat Number1",
                            Row = row,
                            IsReserved = isSeatReserved
                        }
                    }
                }
            }
        };

        return cart;
    }

    public static Offer GetOfferWithItems(Guid offerId, Guid eventId)
    {
        return new Offer
        {
            Id = offerId,
            EventId = eventId,
            Price = new Price
            {
                Amount = 100m
            },
            OfferType = "Test Offer Type2",
            Seat = new Seat
            {
                SeatNumber = "Test Seat Number2",
                Row = new Row
                {
                    Number = "Test Row Number2",
                    Section = new Section
                    {
                        Name = "Test Section Name2",
                        Manifest = new Manifest
                        {
                            Map = "test map",
                            Venue = new Venue
                            {
                                EventId = eventId,
                                Location = "test location"
                            }
                        }
                    }
                },
                IsReserved = false
            }
        };
    }
}
