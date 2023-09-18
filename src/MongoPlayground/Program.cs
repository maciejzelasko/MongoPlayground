using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoPlayground.Documents;
using MongoPlayground.Infrastructure;
using MongoPlayground.Repositories;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets(typeof(DestinationDocument).Assembly);
builder.Services.AddMongoDb();

var host = builder.Build();
await host.RunAsync();

var repository = host.Services.GetRequiredService<IDestinationsRepo>();
