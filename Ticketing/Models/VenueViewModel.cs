namespace Ticketing.Models;

public record VenueViewModel(Guid Id, string Location, EventViewModel Event, ManifestViewModel Manifest);

public record EventViewModel(Guid Id, string Name, DateTime Date, string Description);

public record ManifestViewModel(Guid Id, string Map);
