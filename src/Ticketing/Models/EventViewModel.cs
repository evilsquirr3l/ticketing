namespace Ticketing.Models;

public record EventViewModel(Guid Id, string Name, DateTimeOffset Date, string Description);
