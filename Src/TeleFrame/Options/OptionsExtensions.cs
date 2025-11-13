using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeleFrame.Options;

internal static class Options
{
    /// <param name="services">service collection</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Bind and validate options
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <typeparam name="TOptions">options type</typeparam>
        /// <returns></returns>
        public IServiceCollection BindOptions<TOptions>(IConfiguration configuration) 
            where TOptions : class
        {
            services.AddOptions<TOptions>()
                .Bind(configuration)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            return services;
        }
    }
}