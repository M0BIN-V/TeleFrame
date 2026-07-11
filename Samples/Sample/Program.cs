using Microsoft.Extensions.DependencyInjection;
using Sample;
using TeleFrame.Constants;
using TeleFrame.Middlewares;
using TeleFrame.UpdateHandlers;
using TeleFrame.UpdateHandlers.MessageHandlers.VoiceHandlers;

var builder = new TelegramBotBuilder(args);

// Add services to the container
builder.Services.AddScoped<AdminsService>();
builder.Services.AddScoped<BlackListMiddleware>();
builder.Services.AddUpdateLogging();

var bot = builder.Build();

// Configure the Update pipeline
bot.UseUpdateLogging();

// Register the custom BlackList middleware 
bot.Use<BlackListMiddleware>();

// Inline middleware definition
bot.Use(next => (context, ct) =>
{
    Console.WriteLine("Inline middleware called");
    return next(context, ct);
});

// Send a simple text message response
bot.MapCommand("/start", () => "Welcome to TeleFrame bot!");

// Reply to a command
bot.MapCommand("/hi", () => Results.Reply("Hello world"));

// Use UpdateContext to gain full control over the bot client and context data
bot.MapCommand("/help", (UpdateContext ctx) =>
{
    var userName = ctx.Update.Message!.From!.Username;

    return Results.Reply(
        $"Hello {userName}, I am the TeleFrame demo bot!",
        messageEffect: MessageEffects.Heart);
});

// Handle specific update types with full control (e.g., notifying admins)
bot.MapUpdate(UpdateType.ChatBoost, (UpdateContext ctx, AdminsService service) =>
{
    var adminIds = service.Admins.Select(x => x.Id).ToArray();

    // Send a notification message to all admins 
    foreach (var adminId in adminIds)
        ctx.Client.SendMessage(adminId, "Channel boosted");
});

// Handle edited messages using the UpdateContext
bot.MapUpdate(UpdateType.EditedMessage, (UpdateContext ctx) =>
{
    var message = ctx.Update.EditedMessage!;
    return Results.Reply($"You edited your message: {message.Text}");
});

// Handle incoming voice messages
bot.MapVoice(() => Results.Reply("You sent a voice message!"));

// Start a conversation state workflow
bot.MapCommand("/verify", (IStateManager stateManager) =>
{
    stateManager.SetState("awaiting_phone_number");
    return "Please enter your phone number to verify your account.";
});

// Filter messages based on specific types and conversation states
bot.MapMessage(MessageType.Text, (UpdateContext ctx, IStateManager stateManager) =>
    {
        var input = ctx.Update.Message!.Text!;

        if (input is "/cancel")
        {
            stateManager.ClearState();
            return Results.Reply("Operation canceled.");
        }

        // Validate the format of the phone number
        if (!Regex.Match(input, @"^\+?\d{10,15}$").Success)
            return Results.Reply("❌ Invalid phone number. Please enter a valid phone number.");

        // TODO: Implement your verification business logic here

        stateManager.ClearState();
        return Results.Reply("✅ Your account has been verified.");
    })
    .RequireState("awaiting_phone_number");

bot.Run();