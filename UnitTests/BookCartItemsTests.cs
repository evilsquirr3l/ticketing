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

public class BookCartItemsTests
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
    public async Task Book_MediatrReturnsPayment_ReturnsPayment()
    {
        var mediator = new Mock<IMediator>();
        var cartId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var paymentViewModel = new PaymentViewModel(paymentId, 100, DateTime.Now);
        mediator.Setup(x => x.Send(It.Is<BookCartItems.BookCartItemsCommand>(x => x.CartId == cartId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentViewModel);
        var controller = new BookCartItems(mediator.Object);

        var result = await controller.Book(cartId);
        var createdResult = (Created<PaymentViewModel>)result.Result;

        mediator.VerifyAll();
        Assert.That(createdResult.Location, Is.EqualTo($"payments/{paymentId}/complete"));
        Assert.That(createdResult.Value, Is.Not.Null);
        Assert.That(createdResult.Value, Is.InstanceOf<PaymentViewModel>());
        Assert.That(createdResult.Value, Is.EqualTo(paymentViewModel));
    }

    [Test]
    public async Task Book_MediatrReturnsNull_ReturnsNotFound()
    {
        var mediator = new Mock<IMediator>();
        var cartId = Guid.NewGuid();
        mediator.Setup(x => x.Send(It.Is<BookCartItems.BookCartItemsCommand>(x => x.CartId == cartId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentViewModel)null);
        var controller = new BookCartItems(mediator.Object);

        var result = await controller.Book(cartId);

        mediator.VerifyAll();
        Assert.That(result.Result, Is.InstanceOf<NotFound>());
    }

    [Test]
    public async Task Handle_DatabaseHasValidCartWithItems_ReturnsPaymentWithCorrectAmount()
    {
        var cartId = Guid.NewGuid();
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Carts.AddAsync(GetCartWithItems(cartId));
        await dbContext.SaveChangesAsync();
        var handler = new BookCartItems.BookCartItemsCommandHandler(dbContext);

        var result = await handler.Handle(new BookCartItems.BookCartItemsCommand(cartId), CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<PaymentViewModel>());
        Assert.That(result!.Amount, Is.EqualTo(300));
    }

    [Test]
    public async Task Handle_DatabaseHasValidCartWithItems_AllSeatsBecomeReserved()
    {
        var cartId = Guid.NewGuid();
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Carts.AddAsync(GetCartWithItems(cartId));
        await dbContext.SaveChangesAsync();
        var handler = new BookCartItems.BookCartItemsCommandHandler(dbContext);

        await handler.Handle(new BookCartItems.BookCartItemsCommand(cartId), CancellationToken.None);
        var seats = dbContext.Seats.ToList();

        Assert.That(seats.TrueForAll(x => x.IsReserved), Is.True);
    }

    [Test]
    public async Task Handle_DatabaseDoesntHaveCart_ReturnsNull()
    {
        var cartId = Guid.NewGuid();
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        var handler = new BookCartItems.BookCartItemsCommandHandler(dbContext);

        var result = await handler.Handle(new BookCartItems.BookCartItemsCommand(cartId), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Handle_DatabaseHasCartWithoutItems_ReturnsNull()
    {
        var cartId = Guid.NewGuid();
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Carts.AddAsync(new Cart
        {
            Id = cartId,
            Customer = new Customer
            {
                Email = "test@gmail.com",
                Name = "Test Customer"
            }
        });
        await dbContext.SaveChangesAsync();
        var handler = new BookCartItems.BookCartItemsCommandHandler(dbContext);

        var result = await handler.Handle(new BookCartItems.BookCartItemsCommand(cartId), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    private static Cart GetCartWithItems(Guid cartId)
    {
        var @event = new Event
        {
            Id = Guid.NewGuid(),
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
                        Event = @event,
                        Price = new Price
                        {
                            Amount = 100m
                        },
                        OfferType = "Test Offer Type1",
                        Seat = new Seat
                        {
                            SeatNumber = "Test Seat Number1",
                            Row = row
                        }
                    }
                },
                new()
                {
                    Offer = new Offer
                    {
                        Event = @event,
                        Price = new Price
                        {
                            Amount = 200m
                        },
                        OfferType = "Test Offer Type2",
                        Seat = new Seat
                        {
                            SeatNumber = "Test Seat Number2",
                            Row = row
                        }
                    }
                }
            }
        };
        return cart;
    }
}
