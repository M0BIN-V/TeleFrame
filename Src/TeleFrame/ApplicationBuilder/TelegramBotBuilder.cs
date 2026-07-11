using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeleFrame.Services.StateManager;
using Telegram.Bot.Polling;

namespace TeleFrame.ApplicationBuilder;

public class TelegramBotBuilder : IHostApplicationBuilder
{
    readonly HostApplicationBuilder _hostBuilder;

    readonly ProcessingOptions _processingOptions = new()
    {
        WorkerCount = System.Environment.ProcessorCount * 2,
        QueueCapacity = System.Environment.ProcessorCount * 2 * 4
    };

    readonly ReceiverOptions _receiverOptions = new() { DropPendingUpdates = true };

    public TelegramBotBuilder(string[] args , string configFileName = "botConfig.json")
    {
        _hostBuilder = Host.CreateApplicationBuilder(args);

        _hostBuilder.Configuration.AddJsonFile(configFileName, true, true);
        _hostBuilder.Configuration.AddUserSecrets(System.Reflection.Assembly.GetEntryAssembly()!);

        RegisterServices();
    }

    public IDictionary<object, object> Properties => new Dictionary<object, object>();
    public IConfigurationManager Configuration => _hostBuilder.Configuration;
    public IHostEnvironment Environment => _hostBuilder.Environment;
    public ILoggingBuilder Logging => _hostBuilder.Logging;
    public IMetricsBuilder Metrics => _hostBuilder.Metrics;
    public IServiceCollection Services => _hostBuilder.Services;

    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null)
        where TContainerBuilder : notnull
    {
        _hostBuilder.ConfigureContainer(factory, configure);
    }

    void RegisterServices()
    {
        Services.BindOptions<TelegramBotOptions>(_hostBuilder.Configuration);
        Services.AddSingleton<ITelegramBotClient>(sp =>
            new TelegramBotClient(sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value.Token));
        Services.AddScoped<UpdateContext>();
        Services.AddScoped<IStateManager, MemoryStateManager>();
        Services.AddMemoryCache();
    }

    public TelegramBotBuilder ConfigureReceiver(Action<ReceiverOptions> config)
    {
        config(_receiverOptions);
        return this;
    }

    public TelegramBotBuilder ProcessingOptions(Action<ProcessingOptions> config)
    {
        config(_processingOptions);
        return this;
    }

    public TelegramBotApplication Build()
    {
        return new TelegramBotApplication(_hostBuilder.Build(), _receiverOptions, _processingOptions);
    }
}