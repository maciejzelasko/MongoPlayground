﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MongoPlayground.Infrastructure.Seed;

internal class MongoDataSeeder : IHostedService
{
    private readonly MongoOptions _options;
    private IEnumerable<IMongoSeeder> _mongoSeeders;

    public MongoDataSeeder(IOptions<MongoOptions> options, IEnumerable<IMongoSeeder> mongoSeeders)
    {
        _mongoSeeders = mongoSeeders;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.ShouldSeedData)
            return;

        foreach (var seeder in _mongoSeeders)
            await seeder.Seed(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var seeder in _mongoSeeders)
            await seeder.Cleanup(cancellationToken);
    }
}