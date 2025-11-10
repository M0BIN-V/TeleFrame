using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeleFrame.Options;

public static class OptionsExtensions
{
    /// <summary>
    ///     Bind and validate options
    /// </summary>
    /// <param name="services">service collection</param>
    /// <param name="configuration">configuration</param>
    /// <typeparam name="TOptions">options type</typeparam>
    /// <returns></returns>
    public static IServiceCollection BindOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}