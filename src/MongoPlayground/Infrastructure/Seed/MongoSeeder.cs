﻿using MongoDB.Bson;
using MongoDB.Driver;
using MongoPlayground.Documents;
using DestinationId = MongoPlayground.Documents.DestinationId;

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
    private static readonly IReadOnlyCollection<(string Name, string CountryName, string CountryCode)> Destinations = new[]
    {
        ("Dubai", "United Arab Emirates", "UAE"),
        ("Paris", "France", "FR"),
        ("Hilton Warsaw", "Poland", "PL"),
        ("Jumeirah Beach Hotel", "United Arab Emirates", "UAE"),
    };

    public DestinationsSeeder(IMongoDatabase database) : base(database)
    {
    }

    public override async Task Seed(CancellationToken cancellationToken)
    {
        var collections = await Database.ListCollectionNamesAsync(new ListCollectionNamesOptions
        {
            Filter = new BsonDocument("name", DestinationDocument.CollectionName)
        }, cancellationToken);
        if (!await collections.AnyAsync(cancellationToken: cancellationToken))
        {
            await Database.CreateCollectionAsync(DestinationDocument.CollectionName, cancellationToken: cancellationToken);
            var collection = Database.GetCollection<DestinationDocument>(DestinationDocument.CollectionName);
            var destinations = new List<DestinationDocument>();
            foreach (var (destination, index) in Destinations.Select((summary, index) => (summary, index)))
            {
                destinations.Add(new DestinationDocument
                {
                    Id = DestinationId.New(),
                    Name = destination.Name,
                    CountryName = destination.CountryName,
                    CountryCode = destination.CountryCode,
                    Address= new DestinationAddress
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

    public override Task Cleanup(CancellationToken cancellationToken) =>
        Database.DropCollectionAsync(DestinationDocument.CollectionName, cancellationToken);
}