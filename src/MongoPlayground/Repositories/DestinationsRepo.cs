using MongoDB.Driver;
using MongoPlayground.Entities;

namespace MongoPlayground.Repositories;

interface IDestinationsRepo
{
    
}

internal sealed class DestinationsRepo : IDestinationsRepo
{
    private readonly IMongoCollection<Destination> _collection;

    public DestinationsRepo(IMongoDatabase database)
    {
        _collection = database.GetCollection<Destination>(Destination.CollectionName);
    }
}