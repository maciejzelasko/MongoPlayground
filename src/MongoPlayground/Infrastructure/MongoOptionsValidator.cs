using Microsoft.Extensions.Options;

namespace MongoPlayground.Infrastructure;

internal sealed class MongoOptionsValidator : IValidateOptions<MongoOptions>
{
    public ValidateOptionsResult Validate(string? name, MongoOptions options)
    {
        if (string.IsNullOrEmpty(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail("Connection string is required");
        }
        if (string.IsNullOrEmpty(options.Database))
        {
            return ValidateOptionsResult.Fail("Database is required");
        }

        return ValidateOptionsResult.Success;
    }
}
