using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Ticketing.Data;
using Ticketing.Data.Entities;
using Ticketing.Features.Events;
using Ticketing.Models;

namespace UnitTests;

public class GetEventsTests
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
    public async Task GetAll_CallsMediatr_ReturnsEvents()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<GetEvents.GetEventsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<EventViewModel>
            (
                new List<EventViewModel>
                {
                    new(Guid.NewGuid(), "Test Event", DateTime.Now, "Test Description")
                },
                new Page
                {
                    Count = 1,
                    Skip = 0,
                    Take = 1,
                    Total = 1
                }
            ));
        var controller = new GetEvents(mediator.Object);

        var result = await controller.GetAll(0, 50);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<Ok<PaginatedResult<EventViewModel>>>());

        var okResult = (Ok<PaginatedResult<EventViewModel>>)result;
        Assert.That(okResult.Value, Is.Not.Null);
        Assert.That(okResult.Value.Data.Count, Is.EqualTo(1));
        Assert.That(okResult.Value.Data.FirstOrDefault()?.Name, Is.EqualTo("Test Event"));
        Assert.That(okResult.Value.Data.FirstOrDefault()?.Description, Is.EqualTo("Test Description"));
        Assert.That(okResult.Value.Page.Count, Is.EqualTo(1));
        Assert.That(okResult.Value.Page.Total, Is.EqualTo(1));
        Assert.That(okResult.Value.Page.Take, Is.AtLeast(1));
    }

    [TestCase(0, 50)]
    [TestCase(10, 200)]
    public async Task GetAll_CallsMediatrWithSkipTakeParameters_ParametersArePassedToMediatr(int skip, int take)
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.Is<GetEvents.GetEventsQuery>(y => y.Skip == skip && y.Take == take),
            It.IsAny<CancellationToken>()));
        var controller = new GetEvents(mediator.Object);
        
        await controller.GetAll(skip, take);

        mediator.VerifyAll();
    }
    
    [Test]
    public async Task GetAll_DatabaseHasEvents_ReturnsEventsFromDatabase()
    {
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        var events = new List<Event>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Test Event",
                Date = DateTime.UtcNow,
                Description = "Test Description"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Test Event 2",
                Date = DateTime.UtcNow,
                Description = "Test Description 2"
            }
        };
        await dbContext.Events.AddRangeAsync(events);
        await dbContext.SaveChangesAsync();
        
        var getEventsQueryHandler = new GetEvents.GetEventsQueryHandler(dbContext);
        var result = await getEventsQueryHandler.Handle(new GetEvents.GetEventsQuery(0, 50), CancellationToken.None);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data.Count, Is.EqualTo(2));
        Assert.That(result.Data.FirstOrDefault()?.Name, Is.EqualTo("Test Event"));
        Assert.That(result.Data.FirstOrDefault()?.Description, Is.EqualTo("Test Description"));
        Assert.That(result.Data.LastOrDefault()?.Name, Is.EqualTo("Test Event 2"));
        Assert.That(result.Data.LastOrDefault()?.Description, Is.EqualTo("Test Description 2"));
        Assert.That(result.Page.Count, Is.EqualTo(2));
        Assert.That(result.Page.Total, Is.EqualTo(2));
        Assert.That(result.Page.Take, Is.AtLeast(1));
    }

    [Test]
    public async Task GetAll_DatabaseDoesntHaveEvents_ReturnsEmptyData()
    {
        await using var dbContext = new TicketingDbContext(_dbContextOptions);
        await dbContext.Database.EnsureCreatedAsync();
        
        var getEventsQueryHandler = new GetEvents.GetEventsQueryHandler(dbContext);
        
        var result = await getEventsQueryHandler.Handle(new GetEvents.GetEventsQuery(0, 50), CancellationToken.None);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data.Count, Is.EqualTo(0));
        Assert.That(result.Page.Count, Is.EqualTo(0));
        Assert.That(result.Page.Total, Is.EqualTo(0));
        Assert.That(result.Page.Take, Is.AtLeast(1));
    }
}
