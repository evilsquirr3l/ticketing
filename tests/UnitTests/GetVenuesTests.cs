using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Ticketing.Data;
using Ticketing.Data.Entities;
using Ticketing.Features.Venues;
using Ticketing.Models;

namespace UnitTests;

public class GetVenuesTests
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
    public async Task GetAllVenues_CallsMediatr_ReturnsVenues()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<GetVenues.GetVenuesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<VenueViewModel>
            (
                new List<VenueViewModel>
                {
                    new(Guid.NewGuid(), "Test location", new EventViewModel(Guid.NewGuid(), "Test Event", DateTime.Now, "Test Description"), new ManifestViewModel(Guid.NewGuid(), "test manifest map"))
                },
                new Page
                {
                    Count = 1,
                    Skip = 0,
                    Take = 1,
                    Total = 1
                }
            ));
        var controller = new GetVenues(mediator.Object);

        var result = await controller.GetAllVenues(0, 50);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<Ok<PaginatedResult<VenueViewModel>>>());

        var okResult = (Ok<PaginatedResult<VenueViewModel>>)result;
        Assert.That(okResult.Value, Is.Not.Null);
        Assert.That(okResult.Value.Data.Count, Is.EqualTo(1));
    }

    [TestCase(0, 50)]
    [TestCase(10, 200)]
    public async Task GetAllVenues_CallsMediatrWithSkipTakeParameters_ParametersArePassedToMediatr(int skip, int take)
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.Is<GetVenues.GetVenuesQuery>(y => y.Skip == skip && y.Take == take),
            It.IsAny<CancellationToken>()));
        var controller = new GetVenues(mediator.Object);
        
        await controller.GetAllVenues(skip, take);

        mediator.VerifyAll();
    }
    
    [Test]
    public async Task GetAllVenues_DatabaseHasVenues_ReturnsVenues()
    {
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var venue = new Venue
        {
            Event = new Event
            {
                Id = Guid.NewGuid(),
                Name = "Test Event",
                Date = DateTime.UtcNow,
                Description = "Test Description"
            },
            Location = "Test location",
            Manifest = new Manifest
            {
                Id = Guid.NewGuid(),
                Map = "test manifest map"
            },
        };
        await dbContext.Venues.AddAsync(venue);
        await dbContext.SaveChangesAsync();
        
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<GetVenues.GetVenuesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<VenueViewModel>
            (
                new List<VenueViewModel>
                {
                    new(Guid.NewGuid(), "Test location", new EventViewModel(Guid.NewGuid(), "Test Event", DateTime.Now, "Test Description"), new ManifestViewModel(Guid.NewGuid(), "test manifest map"))
                },
                new Page
                {
                    Count = 1,
                    Skip = 0,
                    Take = 1,
                    Total = 1
                }
            ));
        var controller = new GetVenues(mediator.Object);

        var result = await controller.GetAllVenues(0, 50);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<Ok<PaginatedResult<VenueViewModel>>>());

        var okResult = (Ok<PaginatedResult<VenueViewModel>>)result;
        Assert.That(okResult.Value, Is.Not.Null);
        Assert.That(okResult.Value.Data.Count, Is.EqualTo(1));
        Assert.That(okResult.Value.Data.FirstOrDefault()?.Location, Is.EqualTo("Test location"));
        Assert.That(okResult.Value.Data.FirstOrDefault()?.Event.Name, Is.EqualTo("Test Event"));
        Assert.That(okResult.Value.Data.FirstOrDefault()?.Event.Description, Is.EqualTo("Test Description"));
        Assert.That(okResult.Value.Data.FirstOrDefault()?.Manifest.Map, Is.EqualTo("test manifest map"));
        Assert.That(okResult.Value.Page.Count, Is.EqualTo(1));
        Assert.That(okResult.Value.Page.Total, Is.EqualTo(1));
        Assert.That(okResult.Value.Page.Take, Is.AtLeast(1));
    }

    [Test]
    public async Task GetAllVenues_DatabaseDoesntHaveVenues_ReturnsEmptyData()
    {
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<GetVenues.GetVenuesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<VenueViewModel>
            (
                new List<VenueViewModel>(),
                new Page
                {
                    Count = 0,
                    Skip = 0,
                    Take = 1,
                    Total = 0
                }
            ));
        var controller = new GetVenues(mediator.Object);

        var result = await controller.GetAllVenues(0, 50);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<Ok<PaginatedResult<VenueViewModel>>>());

        var okResult = (Ok<PaginatedResult<VenueViewModel>>)result;
        Assert.That(okResult.Value, Is.Not.Null);
        Assert.That(okResult.Value.Data.Count, Is.EqualTo(0));
        Assert.That(okResult.Value.Page.Count, Is.EqualTo(0));
        Assert.That(okResult.Value.Page.Total, Is.EqualTo(0));
        Assert.That(okResult.Value.Page.Take, Is.AtLeast(1));
    }
}
