using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Search;
using MongoPlayground.Documents;

namespace MongoPlayground.Repositories;

interface IDestinationsRepo
{
    Task<BulkWriteResult<DestinationDocument>> BulkWriteDestinationsAsync(
        IEnumerable<DestinationDocument> models,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DestinationDocument>> GetDestinationsSuggestionsAsync(
        string query,
        string userId,
        IReadOnlyCollection<string> destinationTypes,
        CancellationToken cancellationToken);
}

internal sealed class DestinationsRepo : IDestinationsRepo
{
    private readonly IMongoDatabase _database;

    public DestinationsRepo(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task<BulkWriteResult<DestinationDocument>> BulkWriteDestinationsAsync(
        IEnumerable<DestinationDocument> models,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(models);

        var requests = models.Select(model => new InsertOneModel<DestinationDocument>(model));

        var bulkWriteOptions = new BulkWriteOptions
        {
            IsOrdered = false
        };

        var collection = _database.GetCollection<DestinationDocument>(DestinationDocument.CollectionName);
        var result = await collection.BulkWriteAsync(requests, bulkWriteOptions, cancellationToken);

        return result;
    }

    public async Task<IReadOnlyCollection<DestinationDocument>> GetDestinationsSuggestionsAsync(
        string query,
        string userId,
        IReadOnlyCollection<string> destinationTypes,
        CancellationToken cancellationToken)
    {
        var collection = _database.GetCollection<DestinationDocument>(DestinationDocument.CollectionName);

        var eventCollection = _database.GetCollection<SearchEventDocument>("SearchEvents");

        var eventResult = await eventCollection.Find(
                Builders<SearchEventDocument>.Filter
                    .Gte(doc => doc.CreatedDate, DateTime.UtcNow.AddDays(-30)) &
                    Builders<SearchEventDocument>.Filter.Eq(doc => doc.UserId, userId)
                )
            .ToListAsync(cancellationToken: cancellationToken);

        var searchScoreBuilder = new SearchScoreDefinitionBuilder<DestinationDocument>();
        var searchDefinitions = new List<SearchDefinition<DestinationDocument>>
        {
            Builders<DestinationDocument>.Search
                .Autocomplete(d => d.Name, query),
            Builders<DestinationDocument>.Search
                .Text(d => d.CountryName, query,
                    score: searchScoreBuilder.Boost(3)),
            Builders<DestinationDocument>.Search
                .Text(d => d.Address.Line2, query),
            Builders<DestinationDocument>.Search
                .Phrase(d => d.Name, query,
                    score: searchScoreBuilder.Boost(5))
        };

        var results = await collection.Aggregate()
            .Search(Builders<DestinationDocument>.Search
                    .Compound()
                    .Filter(
                        Builders<DestinationDocument>.Search
                        .Text(d => d.DestinationTypeCode, new MultiSearchQueryDefinition(destinationTypes)),
                        Builders<DestinationDocument>.Search
                        .Equals(d => d.IsDeleted, false))
                    .MinimumShouldMatch(1)
        .Should(searchDefinitions),
                indexName: "AutoCompleteDestinations")
            .Project<SearchResult>(Builders<DestinationDocument>.Projection
                .Include(d => d.Id)
                .Include(d => d.Name)
                .Include(d => d.Address)
                .Include(d => d.CountryCode)
                .Include(d => d.CountryName)
                .MetaSearchScore("score"))
            .Limit(100)
            .ToListAsync(cancellationToken: cancellationToken);

        var userEventSearchDefinitions = eventResult
            .GroupBy(r => r.DestinationId)
            .Select(r => new UserEventRecord(r.Key, 0.1 * r.Count()));

        var joinedResults = results
            .GroupJoin(userEventSearchDefinitions,
                r => r.Id,
                u => u.Id,
                (r, u) => new { Result = r, UserEventSearchDefinitions = u })
            .SelectMany(r => r.UserEventSearchDefinitions.DefaultIfEmpty(),
                (r, u) => new
                {
                    r.Result,
                    Score = r.Result.Score + (u?.Boost ?? 0)
                }).ToList();

        var final = joinedResults
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.Result.Name)
            .Select(r => r.Result)
            .ToList();

        return final;
    }

    private record UserEventRecord(string Id, double Boost);
}

[BsonIgnoreExtraElements]
public class SearchEventDocument
{
    public string DestinationId { get; init; }
    public string? UserId { get; init; }
    public string? CompanyId { get; init; }
    public DateTime CreatedDate { get; init; }
}