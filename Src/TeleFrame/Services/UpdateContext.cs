namespace TeleFrame.Services;

public class UpdateContext(IServiceProvider serviceProvider, ITelegramBotClient botClient)
{
    public ITelegramBotClient Client { get; } = botClient;
    public IServiceProvider Services { get; } = serviceProvider;
    public Update Update { get; set; } = null!;
}