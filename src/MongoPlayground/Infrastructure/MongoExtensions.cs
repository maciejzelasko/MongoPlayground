using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoPlayground.Infrastructure.Seed;

namespace MongoPlayground.Infrastructure;

internal static class MongoExtensions
{
    internal static IServiceCollection AddMongoDb(this IServiceCollection services)
    {
        services
            .AddOptions<MongoOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection(MongoOptions.Section).Bind(options);
            })
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<MongoOptions>, MongoOptionsValidator>();

        services.AddSingleton<IMongoClient>(provider =>
        {
            var mongoSettings = provider.GetRequiredService<IOptions<MongoOptions>>().Value;
            return new MongoClient(mongoSettings.ConnectionString);
        });
        services.AddSingleton(provider =>
        {
            var mongoSettings = provider.GetRequiredService<IOptions<MongoOptions>>().Value;
            var client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoSettings.Database);
        });

        return services;
    }

    public static IServiceCollection AddMongoSeeder(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssemblyOf<MongoDataSeeder>()
                .AddClasses(c => c.AssignableTo<IMongoSeeder>())
                .AsImplementedInterfaces());
        services.AddHostedService<MongoDataSeeder>();
        return services;
    }
}