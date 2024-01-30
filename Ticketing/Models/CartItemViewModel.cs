namespace Ticketing.Models;

public record CartItemViewModel(Guid Id, Guid CartId, Guid OfferId, DateTimeOffset CreatedAt);
