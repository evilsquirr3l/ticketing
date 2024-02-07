namespace Ticketing.Models;

public record VenueViewModel(Guid Id, string Location, EventViewModel Event, ManifestViewModel Manifest);
