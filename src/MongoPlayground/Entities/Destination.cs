namespace MongoPlayground.Entities;

internal class Destination
{
    public const string CollectionName = "Destinations";

    public DestinationId Id { get; init; }

    public required string Name { get; init; }

    public string Description { get; init; }

    public Coordinates Coordinates { get; init; }
}

public record Coordinates(double Latitude, double Longitude);

[StronglyTypedId(true, StronglyTypedIdBackingType.Guid, StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct DestinationId
{
}