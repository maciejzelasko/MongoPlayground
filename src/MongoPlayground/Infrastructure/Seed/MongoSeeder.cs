using MongoDB.Bson;
using MongoDB.Driver;
using MongoPlayground.Entities;

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
    private static readonly IReadOnlyCollection<(string Name, string Description)> Destinations = new[]
    {
        ("Dubai", "Test description"),
        ("Paris", "Test description"),
        ("Hilton Warsaw", "Test description"),
        ("Jumeirah Beach Hotel", "Test description"),
    };

    public DestinationsSeeder(IMongoDatabase database) : base(database)
    {
    }

    public override async Task Seed(CancellationToken cancellationToken)
    {
        var collections = await Database.ListCollectionNamesAsync(new ListCollectionNamesOptions
        {
            Filter = new BsonDocument("name", Destination.CollectionName)
        }, cancellationToken);
        if (!await collections.AnyAsync(cancellationToken: cancellationToken))
        {
            await Database.CreateCollectionAsync(Destination.CollectionName, cancellationToken: cancellationToken);
            var collection = Database.GetCollection<Destination>(Destination.CollectionName);
            var destinations = new List<Destination>();
            foreach (var (destination, index) in Destinations.Select((summary, index) => (summary, index)))
            {
                destinations.Add(new Destination
                {
                    Id = DestinationId.New(),
                    Name = destination.Name,
                    Description = destination.Description,
                });
            }
            await collection.InsertManyAsync(destinations, cancellationToken: cancellationToken);
        }
    }

    public override Task Cleanup(CancellationToken cancellationToken) =>
        Database.DropCollectionAsync(Destination.CollectionName, cancellationToken);
}