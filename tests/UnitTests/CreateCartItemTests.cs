using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Ticketing.Data;
using Ticketing.Features.CartItems;
using Ticketing.Models;

namespace UnitTests;

[TestFixture]
[TestOf(typeof(CreateCartItem))]
public class CreateCartItemTests
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    private DbContextOptions<TicketingDbContext> _dbContextOptions = null!;
    private Mock<TimeProvider> _timeProvider;
    private DateTimeOffset _utcNow = new(2045, 1, 1, 1, 1, 1, TimeSpan.Zero);

    [OneTimeSetUp]
    public async Task Setup()
    {
        _timeProvider = new Mock<TimeProvider>();
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(_utcNow);
        _timeProvider.Setup(x => x.LocalTimeZone).Returns(TimeZoneInfo.Utc);

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
        var cartItemViewModel = new CartItemViewModel(Guid.NewGuid(), offerId, eventId, _utcNow);
        mediator.Setup(x =>
                x.Send(
                    It.Is<CreateCartItem.CreateCartItemCommand>(x =>
                        x.CartId == cartId && x.OfferId == offerId && x.EventId == eventId),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItemViewModel);
        var controller = new CreateCartItem(mediator.Object);

        var result =
            await controller.Create(cartId, new CreateCartItem.CreateCartItemCommand(cartId, offerId, eventId), CancellationToken.None);
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
        mediator.Setup(x =>
                x.Send(
                    It.Is<CreateCartItem.CreateCartItemCommand>(x =>
                        x.CartId == cartId && x.OfferId == offerId && x.EventId == eventId),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound("", "Cart, Offer or Event not found."));
        var controller = new CreateCartItem(mediator.Object);

        var result =
            await controller.Create(cartId, new CreateCartItem.CreateCartItemCommand(cartId, offerId, eventId), CancellationToken.None);
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
        await dbContext.Carts.AddAsync(
            FakeItemsFactory.GetCartWithItems(cartId, offerId, eventId, wasOfferTaken: false));
        await dbContext.SaveChangesAsync();
        var handler = new CreateCartItem.CreateCartItemCommandHandler(dbContext, _timeProvider.Object);

        var result = await handler.Handle(new CreateCartItem.CreateCartItemCommand(cartId, offerId, eventId), CancellationToken.None);

        Assert.That(result.FirstError.Type, Is.EqualTo(ErrorType.Conflict));
        Assert.That(result.FirstError.Description, Is.EqualTo("This offer was already taken."));
    }
    
    [Test]
    public async Task Handle_DatabaseHasValidCartWithTakenOffer_ReturnsConflict()
    {
        var cartId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Carts.AddAsync(FakeItemsFactory.GetCartWithItems(cartId, offerId, eventId, wasOfferTaken: true));
        await dbContext.SaveChangesAsync();
        var handler = new CreateCartItem.CreateCartItemCommandHandler(dbContext, _timeProvider.Object);

        var result = await handler.Handle(new CreateCartItem.CreateCartItemCommand(cartId, offerId, eventId), CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ErrorOr<CartItemViewModel>>());
        Assert.That(result.FirstError.Type, Is.EqualTo(ErrorType.Conflict));
        Assert.That(result.FirstError.Description, Is.EqualTo("This seat is already reserved."));
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
        await dbContext.Carts.AddAsync(
            FakeItemsFactory.GetCartWithItems(cartId, offerId, eventId, wasOfferTaken: false));
        await dbContext.Offers.AddAsync(FakeItemsFactory.GetOfferWithItems(secondOfferId, eventId));
        await dbContext.SaveChangesAsync();
        var handler = new CreateCartItem.CreateCartItemCommandHandler(dbContext, _timeProvider.Object);

        var result = await handler.Handle(new CreateCartItem.CreateCartItemCommand(cartId, secondOfferId, eventId),
            CancellationToken.None);

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ErrorOr<CartItemViewModel>>());
        Assert.That(result.Value.OfferId, Is.EqualTo(secondOfferId));
    }

    [Test]
    public async Task Handle_DatabaseHasValidOfferNotInTheCart_ReturnsCartItemWithCreatedAt()
    {
        var cartId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var secondOfferId = Guid.NewGuid();
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Carts.AddAsync(
            FakeItemsFactory.GetCartWithItems(cartId, offerId, eventId, wasOfferTaken: false));
        await dbContext.Offers.AddAsync(FakeItemsFactory.GetOfferWithItems(secondOfferId, eventId));
        await dbContext.SaveChangesAsync();
        var handler = new CreateCartItem.CreateCartItemCommandHandler(dbContext, _timeProvider.Object);

        var result = await handler.Handle(new CreateCartItem.CreateCartItemCommand(cartId, secondOfferId, eventId), CancellationToken.None);

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ErrorOr<CartItemViewModel>>());
        Assert.That(result.Value.CreatedAt, Is.EqualTo(_utcNow));
    }
}
