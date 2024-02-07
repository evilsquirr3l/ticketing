using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SharedModels;
using Testcontainers.PostgreSql;
using Ticketing.Constants;
using Ticketing.Data;
using Ticketing.Data.Entities;
using Ticketing.Features.CartItems;
using Ticketing.Models;
using Ticketing.Settings;
using ServiceBusMessage = Azure.Messaging.ServiceBus.ServiceBusMessage;

namespace UnitTests;

public class BookCartItemsTests
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    private DbContextOptions<TicketingDbContext> _dbContextOptions = null!;
    private Mock<IOutputCacheStore> _store = new();
    private Mock<ServiceBusSender> _sender = new();
    private Mock<IAzureClientFactory<ServiceBusSender>> _client = new();
    private IOptions<ServiceBusSettings> _options = null!;
    private const string QueueName = "test";

    [OneTimeSetUp]
    public async Task Setup()
    {
        await _postgreSqlContainer.StartAsync();
        _dbContextOptions = new DbContextOptionsBuilder<TicketingDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _options = Options.Create(new ServiceBusSettings
        {
            QueueName = QueueName
        });
        
        _client.Setup(x => x.CreateClient(QueueName)).Returns(_sender.Object);
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
        var paymentViewModel = new PaymentViewModel(paymentId, 100, DateTimeOffset.Now);
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
        var handler = new BookCartItems.BookCartItemsCommandHandler(dbContext, _store.Object, _client.Object, _options);

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
        var handler = new BookCartItems.BookCartItemsCommandHandler(dbContext, _store.Object, _client.Object, _options);

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
        var handler = new BookCartItems.BookCartItemsCommandHandler(dbContext, _store.Object, _client.Object, _options);

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
        var handler = new BookCartItems.BookCartItemsCommandHandler(dbContext, _store.Object, _client.Object, _options);

        var result = await handler.Handle(new BookCartItems.BookCartItemsCommand(cartId), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Handle_DatabaseHasValidCartWithItems_InvalidatesCacheWhenCalled()
    {
        var cartId = Guid.NewGuid();
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Carts.AddAsync(GetCartWithItems(cartId));
        await dbContext.SaveChangesAsync();
        var store = new Mock<IOutputCacheStore>();
        store.Setup(x => x.EvictByTagAsync(Tags.Events, It.IsAny<CancellationToken>()));
        var handler = new BookCartItems.BookCartItemsCommandHandler(dbContext, store.Object, _client.Object, _options);

        await handler.Handle(new BookCartItems.BookCartItemsCommand(cartId), CancellationToken.None);

        _store.Verify(x => x.EvictByTagAsync(Tags.Events, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test]
    public async Task Handle_DatabaseHasValidCartWithItems_SendsCorrectMessageToServiceBusWhenCalled()
    {
        var cartId = Guid.NewGuid();
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Carts.AddAsync(GetCartWithItems(cartId));
        await dbContext.SaveChangesAsync();
        _sender.Setup(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()));
        var handler = new BookCartItems.BookCartItemsCommandHandler(dbContext, _store.Object, _client.Object, _options);

        var result = await handler.Handle(new BookCartItems.BookCartItemsCommand(cartId), CancellationToken.None);
        var customer = await dbContext.Customers.FirstAsync();
        var expectedMessage = new Message(result!.Id, "Book", result.PaymentDate!.Value, customer.Email,
            customer.Name, result.Amount);

        _sender.Verify(
            x => x.SendMessageAsync(
                It.Is<ServiceBusMessage>(message => message.Body.ToObjectFromJson<Message>(null) == expectedMessage),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Cart GetCartWithItems(Guid cartId)
    {
        var @event = new Event
        {
            Id = Guid.NewGuid(),
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
