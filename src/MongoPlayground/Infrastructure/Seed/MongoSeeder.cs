using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoPlayground.Documents;

namespace MongoPlayground.Infrastructure.Seed;

internal interface IMongoSeeder
{
    Task Seed(CancellationToken cancellationToken);

    Task Cleanup(CancellationToken cancellationToken);
}

internal abstract class MongoSeeder : IMongoSeeder
{
    protected readonly IMongoDatabase Database;

    protected MongoSeeder(IMongoDatabase database)
    {
        Database = database;
    }

    public abstract Task Seed(CancellationToken cancellationToken);
    public abstract Task Cleanup(CancellationToken cancellationToken);
}

internal sealed class DestinationsSeeder : MongoSeeder
{
    public DestinationsSeeder(IMongoDatabase database) : base(database)
    {
    }

    public override async Task Seed(CancellationToken cancellationToken)
    {
        var collections = await Database.ListCollectionNamesAsync(new ListCollectionNamesOptions
        {
            Filter = new BsonDocument("name", DestinationDocument.CollectionName)
        }, cancellationToken);

        if (!await collections.AnyAsync(cancellationToken))
        {
            await Database.CreateCollectionAsync(DestinationDocument.CollectionName,
                cancellationToken: cancellationToken);
            var collection = Database.GetCollection<DestinationDocument>(DestinationDocument.CollectionName);

            var jsonData = await File.ReadAllTextAsync("Infrastructure\\Seed\\data.json", cancellationToken);
            var destinationsSeed = JsonSerializer.Deserialize<DestinationsSeed>(jsonData);

            var destinations = new List<DestinationDocument>();
            foreach (var (destination, index) in destinationsSeed.Destinations.Select((summary, index) => (summary, index)))
            {
                destinations.Add(new DestinationDocument
                {
                    Id = DestinationId.New(),
                    Name = destination.Name,
                    CountryName = destination.CountryName,
                    CountryCode = destination.CountryCode,
                    Address = new DestinationAddress
                    {
                        Line1 = $"Line 1 {index}",
                        Line2 = $"Line 2 {index}",
                        Line3 = $"Line 3 {index}"
                    },
                    DestinationTypeCode = "H",
                    CreatedDate = DateTime.UtcNow
                });
            }
            await collection.InsertManyAsync(destinations, cancellationToken: cancellationToken);
        }
    }

    public override Task Cleanup(CancellationToken cancellationToken)
    {
        return Database.DropCollectionAsync(DestinationDocument.CollectionName, cancellationToken);
    }
}

public class DestinationsSeed
{
    public IReadOnlyCollection<DestinationItemSeed> Destinations { get; init; }
}

public record DestinationItemSeed(string Name, string CountryName, string CountryCode);