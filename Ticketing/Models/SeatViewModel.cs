namespace Ticketing.Models;

public record SeatViewModel(Guid Id, string SeatNumber, bool IsReserved, Guid RowId);
