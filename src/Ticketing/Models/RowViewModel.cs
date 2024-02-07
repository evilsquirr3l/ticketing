namespace Ticketing.Models;

public record RowViewModel(Guid Id, string Number, IEnumerable<SeatViewModel>? Seats = null);
