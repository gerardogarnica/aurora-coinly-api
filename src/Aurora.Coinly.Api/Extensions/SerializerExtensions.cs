using System.Text.Json.Serialization;

namespace Aurora.Coinly.Api.Extensions;

internal static class SerializerExtensions
{
    internal static IServiceCollection AddStringEnumConverter(this IServiceCollection services)
    {
        services
            .AddMvc()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return services;
    }
}