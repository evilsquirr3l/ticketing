namespace Ticketing.Models;

public record SectionViewModel(Guid Id, string Name, IEnumerable<RowViewModel>? Rows = null);
