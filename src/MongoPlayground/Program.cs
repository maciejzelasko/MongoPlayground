using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoPlayground.Documents;
using MongoPlayground.Infrastructure;
using MongoPlayground.Repositories;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets(typeof(DestinationDocument).Assembly);
builder.Services.AddMongoDb();
builder.Services.AddMongoSeeder();
builder.Services.AddTransient<IDestinationsRepo, DestinationsRepo>();

var host = builder.Build();

var repository = host.Services.GetRequiredService<IDestinationsRepo>();

//await repository.TruncateDestinationsAsync(CancellationToken.None);
var docs = await repository.GetDestinationsSuggestionsAsync("Hilton Poland", Guid.NewGuid().ToString(), new []{ "H" }, CancellationToken.None);

await host.RunAsync();
