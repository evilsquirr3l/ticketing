using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Ticketing.Data;
using Ticketing.Data.Entities;
using Ticketing.Features.CartItems;
using Ticketing.Models;

namespace UnitTests;

[TestFixture]
[TestOf(typeof(CreateCartItem))]
public class CreateCartItemTests
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    private DbContextOptions<TicketingDbContext> _dbContextOptions = null!;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        await _postgreSqlContainer.StartAsync();
        _dbContextOptions = new DbContextOptionsBuilder<TicketingDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options; 
    }
    
    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
    
    [Test]
    public async Task CreateCartItem_MediatrReturnsCartItem_ReturnsCartItem()
    {
        var mediator = new Mock<IMediator>();
        var cartId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var cartItemViewModel = new CartItemViewModel(Guid.NewGuid(), offerId, eventId);
        mediator.Setup(x => x.Send(It.Is<CreateCartItem.CreateCartItemCommand>(x => x.CartId == cartId && x.OfferId == offerId && x.EventId == eventId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItemViewModel);
        var controller = new CreateCartItem(mediator.Object);

        var result = await controller.Create(cartId, new CreateCartItem.CreateCartItemCommand(cartId, offerId, eventId));
        var createdResult = (Created<CartItemViewModel>)result.Result;

        mediator.VerifyAll();
        Assert.That(createdResult.Location, Is.EqualTo($"orders/carts/{cartId}"));
        Assert.That(createdResult.Value, Is.Not.Null);
        Assert.That(createdResult.Value, Is.InstanceOf<CartItemViewModel>());
        Assert.That(createdResult.Value, Is.EqualTo(cartItemViewModel));
    }
    
    [Test]
    public async Task CreateCartItem_MediatrReturnsNull_ReturnsErrorMessage()
    {
        var mediator = new Mock<IMediator>();
        var cartId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        mediator.Setup(x => x.Send(It.Is<CreateCartItem.CreateCartItemCommand>(x => x.CartId == cartId && x.OfferId == offerId && x.EventId == eventId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItemViewModel?)null);
        var controller = new CreateCartItem(mediator.Object);

        var result = await controller.Create(cartId, new CreateCartItem.CreateCartItemCommand(cartId, offerId, eventId));
        var badRequest = (BadRequest<string>)result.Result;

        mediator.VerifyAll();
        Assert.That(badRequest.Value, Is.EqualTo("Cart, Offer or Event not found."));
    }
    
    [Test]
    public async Task Handle_OfferIsAlreadyInTheCart_ThrowsInvalidOperationException()
    {
        var cartId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Carts.AddAsync(GetCartWithItems(cartId, offerId, eventId, isSeatReserved: false));
        await dbContext.SaveChangesAsync();
        var handler = new CreateCartItem.CreateCartItemCommandHandler(dbContext);
        
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(new CreateCartItem.CreateCartItemCommand(cartId, offerId, eventId), CancellationToken.None));
        Assert.That(exception?.Message, Is.EqualTo("This offer is already in the cart."));
    }
    
    [Test]
    public async Task Handle_DatabaseHasValidCartWithReservedSeat_ThrowsInvalidOperationException()
    {
        var cartId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Carts.AddAsync(GetCartWithItems(cartId, offerId, eventId, isSeatReserved: true));
        await dbContext.SaveChangesAsync();
        var handler = new CreateCartItem.CreateCartItemCommandHandler(dbContext);
        
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(new CreateCartItem.CreateCartItemCommand(cartId, offerId, eventId), CancellationToken.None));
        Assert.That(exception?.Message, Is.EqualTo("Seat is already reserved."));
    }
    
    [Test]
    public async Task Handle_DatabaseHasValidOfferNotInTheCart_ReturnsCartItem()
    {
        var cartId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var secondOfferId = Guid.NewGuid();
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Carts.AddAsync(GetCartWithItems(cartId, offerId, eventId, isSeatReserved: false));
        await dbContext.Offers.AddAsync(GetOfferWithItems(secondOfferId, eventId));
        await dbContext.SaveChangesAsync();
        var handler = new CreateCartItem.CreateCartItemCommandHandler(dbContext);
        
        var result = await handler.Handle(new CreateCartItem.CreateCartItemCommand(cartId, secondOfferId, eventId), CancellationToken.None);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<CartItemViewModel>());
        Assert.That(result?.OfferId, Is.EqualTo(secondOfferId));
    }

    private static Cart GetCartWithItems(Guid cartId, Guid offerId, Guid eventId, bool isSeatReserved)
    {
        var @event = new Event
        {
            Id = eventId,
            Name = "Test Event2",
            Date = DateTime.UtcNow,
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
    
    private static Offer GetOfferWithItems(Guid offerId, Guid eventId)
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
