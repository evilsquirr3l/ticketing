using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Ticketing.Data;
using Ticketing.Data.Entities;
using Ticketing.Models;

namespace IntegrationTests;

public class BookCartItemsTests
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    private readonly RedisContainer _redisContainer = new RedisBuilder().Build();
    private DbContextOptions<TicketingDbContext> _dbContextOptions = null!;
    private HttpClient _httpClient = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        await _postgreSqlContainer.StartAsync();
        var databaseConnectionString = _postgreSqlContainer.GetConnectionString();
        
        _dbContextOptions = new DbContextOptionsBuilder<TicketingDbContext>()
            .UseNpgsql(databaseConnectionString)
            .Options;

        await _redisContainer.StartAsync();
        var redisConnectionString = _redisContainer.GetConnectionString();
        var factory = new TestingWebApplicationFactory(databaseConnectionString,
            redisConnectionString);
        
        _httpClient = factory.CreateClient();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _postgreSqlContainer.DisposeAsync();
        _httpClient.Dispose();
    }

    [Test]
    public async Task Book_ValidCartWithItems_ReturnsPaymentWithCorrectAmount()
    {
        var cartId = Guid.NewGuid();
        var cart = GetCart(cartId, withItems: true);
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Carts.AddAsync(cart);
        await dbContext.SaveChangesAsync();

        var response = await _httpClient.PutAsync($"orders/carts/{cartId}/book", null);
        var payment = await response.Content.ReadFromJsonAsync<PaymentViewModel>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(payment, Is.Not.Null);
            Assert.That(payment!.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(payment!.Amount, Is.EqualTo(300m));
            Assert.That(payment!.PaymentDate, Is.EqualTo(DateTime.UtcNow).Within(1).Seconds);
        });
    }

    [Test]
    public async Task Book_ValidCartWithoutItems_ReturnsNotFound()
    {
        var cartId = Guid.NewGuid();
        var cart = GetCart(cartId, withItems: false);
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Carts.AddAsync(cart);
        await dbContext.SaveChangesAsync();

        var response = await _httpClient.PutAsync($"orders/carts/{cartId}/book", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Book_InvalidCart_ReturnsNotFound()
    {
        var response = await _httpClient.PutAsync($"orders/carts/{Guid.NewGuid()}/book", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private static Cart GetCart(Guid cartId, bool withItems = true)
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
            CartItems = withItems
                ? new List<CartItem>
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
                : null
        };
        return cart;
    }
}
