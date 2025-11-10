using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeleFrame.Application;
using TeleFrame.Services;
using TeleFrame.Services.StateManager;
using Telegram.Bot.Polling;

namespace TeleFrame.ApplicationBuilder;

public class TelegramBotApplicationBuilder : IHostApplicationBuilder
{
    readonly HostApplicationBuilder _hostBuilder;
    readonly ReceiverOptions _receiverOptions = new() { DropPendingUpdates = true };

    public TelegramBotApplicationBuilder(string[] args)
    {
        _hostBuilder = Host.CreateApplicationBuilder(args);

        _hostBuilder.Configuration.AddJsonFile("appsettings.json", true, true);

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

    public TelegramBotApplicationBuilder ConfigureReceiver(Action<ReceiverOptions> config)
    {
        config(_receiverOptions);
        return this;
    }

    public TelegramBotApplication Build()
    {
        return new TelegramBotApplication(_hostBuilder.Build(), _receiverOptions);
    }
}