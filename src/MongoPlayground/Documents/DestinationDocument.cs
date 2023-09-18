using MongoDB.Bson.Serialization.Attributes;

namespace MongoPlayground.Documents;

public class DestinationDocument
{
    public const string CollectionName = "Destinations";

    public DestinationId Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public required string CountryName { get; init; }

    public required string CountryCode { get; init; }

    public required DestinationAddress Address { get; init; }

    public Coordinates? Coordinates { get; init; }

    public required string DestinationTypeCode { get; init; }

    public bool IsDeleted { get; init; }

    public required DateTime CreatedDate { get; init; }
}

public class DestinationAddress
{
    public string Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
}

public class SearchResult : DestinationDocument
{
    [BsonElement("score")]
    public double Score { get; set; }
}

