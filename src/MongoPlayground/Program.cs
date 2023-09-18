using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoPlayground.Entities;
using MongoPlayground.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets(typeof(Destination).Assembly);
builder.Services.AddMongoDb();

var host = builder.Build();
host.Run();
