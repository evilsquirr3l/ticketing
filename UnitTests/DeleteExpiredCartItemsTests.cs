using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Ticketing.Data;
using Ticketing.Data.Entities;
using Ticketing.Features.CartItems;
using Ticketing.Settings;

namespace UnitTests;

public class DeleteExpiredCartItemsTests
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    private DbContextOptions<TicketingDbContext> _dbContextOptions = null!;
    private IOptions<CartItemsExpiration> _options = null!;
    private Mock<TimeProvider> _timeProvider;
    private DateTimeOffset _utcNow = new(2045, 1, 1, 1, 1, 1, TimeSpan.Zero);
    private readonly int _cartItemsExpiration = 15;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _timeProvider = new Mock<TimeProvider>();
        _timeProvider.Setup(x => x.LocalTimeZone).Returns(TimeZoneInfo.Utc);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(_utcNow);

        await _postgreSqlContainer.StartAsync();
        _dbContextOptions = new DbContextOptionsBuilder<TicketingDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _options = Options.Create(new CartItemsExpiration { CartItemsExpirationInMinutes = _cartItemsExpiration });
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    [Test]
    public async Task Handle_DatabaseHasExpiredCartItem_DeletesCartItem()
    {
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        var cartId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var cartWithItems = FakeItemsFactory.GetCartWithItems(cartId, offerId, eventId, isSeatReserved: false);
        await dbContext.Carts.AddAsync(cartWithItems);
        var offerWithItems = FakeItemsFactory.GetOfferWithItems(Guid.NewGuid(), eventId);
        await dbContext.Offers.AddAsync(offerWithItems);
        await dbContext.CartItems.AddAsync(new CartItem()
        {
            Cart = cartWithItems,
            Offer = offerWithItems,
            CreatedAt = _utcNow.AddMinutes(-_cartItemsExpiration - 1)
        });
        await dbContext.SaveChangesAsync();
        var handler =
            new DeleteExpiredCartItems.DeleteExpiredCartItemsCommandHandler(dbContext, _timeProvider.Object, _options);

        await handler.Handle(new DeleteExpiredCartItems.DeleteExpiredCartItemsCommand(), CancellationToken.None);

        var cartItems = await dbContext.CartItems.ToListAsync();
        Assert.That(cartItems, Is.Empty);
    }

    [Test]
    public async Task Handle_DatabaseHasValidCartItem_CartItemIsNotDeleted()
    {
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        var cartId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var cartWithItems = FakeItemsFactory.GetCartWithItems(cartId, offerId, eventId, isSeatReserved: false);
        await dbContext.Carts.AddAsync(cartWithItems);
        var offerWithItems = FakeItemsFactory.GetOfferWithItems(Guid.NewGuid(), eventId);
        await dbContext.Offers.AddAsync(offerWithItems);
        await dbContext.CartItems.AddAsync(new CartItem()
        {
            Cart = cartWithItems,
            Offer = offerWithItems,
            CreatedAt = _utcNow.AddMinutes(-_cartItemsExpiration + 1)
        });
        await dbContext.SaveChangesAsync();
        var handler =
            new DeleteExpiredCartItems.DeleteExpiredCartItemsCommandHandler(dbContext, _timeProvider.Object, _options);
        
        await handler.Handle(new DeleteExpiredCartItems.DeleteExpiredCartItemsCommand(), CancellationToken.None);
        
        var cartItems = await dbContext.CartItems.ToListAsync();
        Assert.That(cartItems, Is.Not.Empty);
    }
}
