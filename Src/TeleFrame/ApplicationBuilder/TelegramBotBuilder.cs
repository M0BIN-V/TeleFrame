using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeleFrame.Services.StateManager;
using Telegram.Bot.Polling;

namespace TeleFrame.ApplicationBuilder;

/// <summary>
/// A custom implementation of <see cref="IHostApplicationBuilder"/> tailored for bootstrapping, 
/// configuring, and assembling a <see cref="TelegramBotApplication"/>.
/// </summary>
/// <remarks>
/// This builder encapsulates a standard <see cref="HostApplicationBuilder"/>, applies default configurations 
/// (such as environment-based worker counts and JSON file providers), and wires up essential bot infrastructure services.
/// </remarks>
public class TelegramBotBuilder : IHostApplicationBuilder
{
    readonly HostApplicationBuilder _hostBuilder;

    readonly ProcessingOptions _processingOptions = new()
    {
        WorkerCount = System.Environment.ProcessorCount * 2,
        QueueCapacity = System.Environment.ProcessorCount * 2 * 4
    };

    readonly ReceiverOptions _receiverOptions = new() { DropPendingUpdates = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="TelegramBotBuilder"/> class with command-line arguments 
    /// and a specific configuration file name.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the application entry point.</param>
    /// <param name="configFileName">The target JSON configuration file name. Defaults to "botConfig.json".</param>
    /// <remarks>
    /// The constructor automatically wires up JSON configuration providers, loads entry assembly User Secrets 
    /// for local development security, and triggers internal service registrations.
    /// </remarks>
    public TelegramBotBuilder(string[] args , string configFileName = "botConfig.json")
    {
        _hostBuilder = Host.CreateApplicationBuilder(args);

        _hostBuilder.Configuration.AddJsonFile(configFileName, true, true);
        _hostBuilder.Configuration.AddUserSecrets(System.Reflection.Assembly.GetEntryAssembly()!);

        RegisterServices();
    }

    /// <summary>
    /// Gets a core generic dictionary shared across the application builder to store arbitrary properties.
    /// </summary>
    /// <value>A new empty dictionary instance.</value>
    public IDictionary<object, object> Properties => new Dictionary<object, object>();

    /// <summary>
    /// Gets the application configuration manager utilized to mutate or read configuration values during setup.
    /// </summary>
    /// <value>The underlying <see cref="IConfigurationManager"/> instance.</value>
    public IConfigurationManager Configuration => _hostBuilder.Configuration;

    /// <summary>
    /// Gets information about the current hosting environment (e.g., Development, Staging, Production).
    /// </summary>
    /// <value>The <see cref="IHostEnvironment"/> instance managing environment properties.</value>
    public IHostEnvironment Environment => _hostBuilder.Environment;

    /// <summary>
    /// Gets the logging builder to configure logging providers, filters, and formats.
    /// </summary>
    /// <value>The <see cref="ILoggingBuilder"/> configuration instance.</value>
    public ILoggingBuilder Logging => _hostBuilder.Logging;

    /// <summary>
    /// Gets the metrics builder used to configure OpenTelemetry or custom metric instruments.
    /// </summary>
    /// <value>The <see cref="IMetricsBuilder"/> instance.</value>
    public IMetricsBuilder Metrics => _hostBuilder.Metrics;

    /// <summary>
    /// Gets the central service collection where application dependencies and frameworks are registered.
    /// </summary>
    /// <value>The <see cref="IServiceCollection"/> instance for Dependency Injection.</value>
    public IServiceCollection Services => _hostBuilder.Services;

    /// <summary>
    /// Configures the underlying dependency injection container factory.
    /// </summary>
    /// <typeparam name="TContainerBuilder">The type of the builder to create the container.</typeparam>
    /// <param name="factory">The factory used to initiate container generation.</param>
    /// <param name="configure">The configuration delegate executed against the custom container builder.</param>
    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null)
        where TContainerBuilder : notnull
    {
        _hostBuilder.ConfigureContainer(factory, configure);
    }

    /// <summary>
    /// Registers core framework dependencies required for the Telegram Bot infrastructure to run smoothly.
    /// </summary>
    /// <remarks>
    /// This method registers strongly-typed <see cref="TelegramBotOptions"/>, initializes the <see cref="ITelegramBotClient"/> 
    /// using resolved options tokens, and scopes state management components like <see cref="UpdateContext"/> and <see cref="IStateManager"/>.
    /// </remarks>
    void RegisterServices()
    {
        Services.BindOptions<TelegramBotOptions>(_hostBuilder.Configuration);
        Services.AddSingleton<ITelegramBotClient>(sp =>
            new TelegramBotClient(sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value.Token));
        Services.AddScoped<UpdateContext>();
        Services.AddScoped<IStateManager, MemoryStateManager>();
        Services.AddMemoryCache();
    }

    /// <summary>
    /// Configures options governing the active Telegram polling receiver routine.
    /// </summary>
    /// <param name="config">The configuration delegate managing properties inside <see cref="ReceiverOptions"/>.</param>
    /// <returns>The current <see cref="TelegramBotBuilder"/> instance to support fluent method chaining configurations.</returns>
    public TelegramBotBuilder ConfigureReceiver(Action<ReceiverOptions> config)
    {
        config(_receiverOptions);
        return this;
    }

    /// <summary>
    /// Configures internal worker thread thresholds and buffer metrics for handling incoming update pipelines.
    /// </summary>
    /// <param name="config">The configuration delegate managing properties inside <see cref="ProcessingOptions"/>.</param>
    /// <returns>The current <see cref="TelegramBotBuilder"/> instance to support fluent method chaining configurations.</returns>
    public TelegramBotBuilder ProcessingOptions(Action<ProcessingOptions> config)
    {
        config(_processingOptions);
        return this;
    }

    /// <summary>
    /// Builds and compiles the inner generic host infrastructure into a concrete, executable <see cref="TelegramBotApplication"/>.
    /// </summary>
    /// <returns>A fully configured and initialized <see cref="TelegramBotApplication"/> instance ready to be started.</returns>
    /// <remarks>
    /// Calling this method seals the configuration states and builds the final service provider context.
    /// </remarks>
    public TelegramBotApplication Build()
    {
        return new TelegramBotApplication(_hostBuilder.Build(), _receiverOptions, _processingOptions);
    }
}