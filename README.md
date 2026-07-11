# TeleFrame

A modern, lightweight .NET framework for building Telegram bots with a fluent API and middleware pipeline architecture. Built on top of [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot), TeleFrame provides an intuitive way to handle Telegram updates, manage conversation state, and build complex bot workflows.

## 📋 Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
- [Usage Guide](#usage-guide)
  - [Basic Setup](#basic-setup)
  - [Handling Updates](#handling-updates)
  - [Message Handlers](#message-handlers)
  - [Command Handlers](#command-handlers)
  - [Voice Handlers](#voice-handlers)
  - [State Management](#state-management)
  - [Middleware](#middleware)
  - [Sending Results](#sending-results)
- [Configuration](#configuration)
- [Advanced Features](#advanced-features)
- [Examples](#examples)
- [License](#license)

## ✨ Features

- **Fluent API**: Intuitive builder pattern for registering handlers
- **Middleware Pipeline**: Process updates through a customizable middleware chain
- **State Management**: Built-in memory-based state manager for handling conversation state
- **Type-Safe Handlers**: Lambda-based handlers with full IntelliSense support
- **Update Filtering**: Predicate-based filtering for precise control over update routing
- **Multiple Update Types**: Handle messages, commands, voice, and custom updates
- **Dependency Injection**: Full Microsoft.Extensions.DependencyInjection integration
- **Configuration Support**: Seamless integration with Microsoft.Extensions.Configuration
- **Async Processing**: Non-blocking update processing with configurable worker threads
- **Logging**: Built-in logging support via Microsoft.Extensions.Logging

## 📦 Installation

### Via NuGet Package Manager

```bash
Install-Package TeleFrame
```

### Via .NET CLI

```bash
dotnet add package TeleFrame
```

## 🚀 Quick Start

Here's a minimal bot that responds to `/start` command:

```csharp
using TeleFrame;
using TeleFrame.ApplicationBuilder;

var builder = new TelegramBotApplicationBuilder(args);

var app = builder.Build();

app.MapUpdate(u => u.Update.Type == UpdateType.Message, async (context, ct) =>
{
    if (context.Update.Message?.Text == "/start")
    {
        await context.Client.SendTextMessageAsync(
            context.Update.Message.Chat.Id,
            "Hello! I'm your bot.",
            cancellationToken: ct);
    }
});

await app.RunAsync();
```

## 🎯 Core Concepts

### UpdateContext

The `UpdateContext` is passed to every handler and contains:

- **Update**: The Telegram update object with all its data
- **Client**: The ITelegramBotClient for sending messages
- **Services**: The dependency injection container for accessing services

```csharp
public class UpdateContext
{
    public ITelegramBotClient Client { get; }
    public IServiceProvider Services { get; }
    public Update Update { get; set; }
}
```

### Update Pipeline

Updates flow through a middleware pipeline that you can customize. Each middleware can:

- Inspect or modify the update context
- Handle the update or pass it to the next middleware
- Perform logging, validation, or other cross-cutting concerns

### State Management

State is tracked per user, allowing you to build multi-step conversations and workflows.

## 📖 Usage Guide

### Basic Setup

#### 1. Configure Options

Create `appsettings.json`:

```json
{
  "TelegramBot": {
    "Token": "YOUR_BOT_TOKEN_HERE"
  }
}
```

#### 2. Initialize the Application

```csharp
using TeleFrame;
using TeleFrame.ApplicationBuilder;

var builder = new TelegramBotApplicationBuilder(args);

// Add services
builder.Services.AddLogging();

var app = builder.Build();
```

#### 3. Register Handlers

```csharp
// Register handlers here
app.MapUpdate(...);

// Run the bot
await app.RunAsync();
```

### Handling Updates

Use `MapUpdate` to handle any type of update with a custom predicate:

```csharp
// Handle all message updates
app.MapUpdate(
    u => u.Update.Type == UpdateType.Message,
    async (context, ct) =>
    {
        await context.Client.SendTextMessageAsync(
            context.Update.Message!.Chat.Id,
            "You sent a message!",
            cancellationToken: ct);
    });

// Handle specific update types
app.MapUpdate(UpdateType.CallbackQuery, async (context, ct) =>
{
    var callbackData = context.Update.CallbackQuery?.Data;
    // Handle callback
});
```

### Message Handlers

Handle specific message types:

```csharp
// Handle text messages
app.MapMessage(
    m => m.Type == MessageType.Text,
    async (context, ct) =>
    {
        var text = context.Update.Message!.Text;
        await context.Client.SendTextMessageAsync(
            context.Update.Message.Chat.Id,
            $"You said: {text}",
            cancellationToken: ct);
    });

// Handle photo messages
app.MapMessage(MessageType.Photo, async (context, ct) =>
{
    var photo = context.Update.Message!.Photo!.Last();
    await context.Client.SendTextMessageAsync(
        context.Update.Message.Chat.Id,
        $"Photo received! File ID: {photo.FileId}",
        cancellationToken: ct);
});

// Handle voice messages
app.MapMessage(MessageType.Voice, async (context, ct) =>
{
    var voice = context.Update.Message!.Voice!;
    await context.Client.SendTextMessageAsync(
        context.Update.Message.Chat.Id,
        $"Voice received! Duration: {voice.Duration}s",
        cancellationToken: ct);
});

// Handle document messages
app.MapMessage(MessageType.Document, async (context, ct) =>
{
    var document = context.Update.Message!.Document!;
    await context.Client.SendTextMessageAsync(
        context.Update.Message.Chat.Id,
        $"Document received: {document.FileName}",
        cancellationToken: ct);
});
```

### Command Handlers

Handle bot commands like `/start`, `/help`, etc.:

```csharp
// Handle /start command
app.MapMessage(
    m => m.Text == "/start",
    async (context, ct) =>
    {
        await context.Client.SendTextMessageAsync(
            context.Update.Message!.Chat.Id,
            "Welcome! Type /help for available commands.",
            cancellationToken: ct);
    });

// Handle /help command
app.MapMessage(
    m => m.Text == "/help",
    async (context, ct) =>
    {
        var helpText = """
            Available commands:
            /start - Start the bot
            /help - Show this help message
            /info - Get info about the bot
            """;
        
        await context.Client.SendTextMessageAsync(
            context.Update.Message!.Chat.Id,
            helpText,
            cancellationToken: ct);
    });
```

### Voice Handlers

Handle voice messages specifically:

```csharp
app.MapMessage(MessageType.Voice, async (context, ct) =>
{
    var voice = context.Update.Message!.Voice!;
    var duration = voice.Duration;
    
    await context.Client.SendTextMessageAsync(
        context.Update.Message.Chat.Id,
        $"Voice message received!\nDuration: {duration} seconds",
        cancellationToken: ct);
});
```

### State Management

Manage conversation state to build multi-step workflows:

```csharp
// Register state manager (built-in memory-based implementation)
builder.Services.AddSingleton<IStateManager, MemoryStateManager>();

// Use state in handlers
app.MapMessage(
    m => m.Text == "/register",
    async (context, ct) =>
    {
        var stateManager = context.Services.GetRequiredService<IStateManager>();
        stateManager.SetState("awaiting_name");
        
        await context.Client.SendTextMessageAsync(
            context.Update.Message!.Chat.Id,
            "What's your name?",
            cancellationToken: ct);
    });

// Handle messages when in specific state
app.MapMessage(
    m => m.Type == MessageType.Text,
    async (context, ct) =>
    {
        var stateManager = context.Services.GetRequiredService<IStateManager>();
        
        if (stateManager.State == "awaiting_name")
        {
            var name = context.Update.Message!.Text;
            stateManager.SetState("awaiting_email");
            
            await context.Client.SendTextMessageAsync(
                context.Update.Message.Chat.Id,
                $"Nice to meet you, {name}! What's your email?",
                cancellationToken: ct);
        }
    }).RequireState("awaiting_name");
```

### Middleware

Add custom middleware to process updates before handlers:

```csharp
// Add logging middleware
app.Use(next => async (context, ct) =>
{
    var logger = context.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Processing update: {UpdateId}", context.Update.Id);
    
    await next(context, ct);
    
    logger.LogInformation("Update processed: {UpdateId}", context.Update.Id);
});

// Add authentication middleware
app.Use(next => async (context, ct) =>
{
    var allowedUsers = new[] { 123456789, 987654321 };
    var userId = context.Update.Message?.From?.Id;
    
    if (userId.HasValue && allowedUsers.Contains((int)userId.Value))
    {
        await next(context, ct);
    }
    else
    {
        await context.Client.SendTextMessageAsync(
            context.Update.Message!.Chat.Id,
            "You don't have permission to use this bot.",
            cancellationToken: ct);
    }
});
```

### Sending Results

Use result classes for structured message sending:

```csharp
using TeleFrame.Results;

// Send text message
var result = new TextResult("Hello, World!", chatId: 123456789);
await result.SendAsync(context, ct);

// Send text message with reply
var replyResult = new ReplyResult("Hello!", context.Update.Message!.MessageId);
await replyResult.SendAsync(context, ct);

// Custom message result
var customResult = new MessageResult()
{
    // Configure message options
    await customResult.SendAsync(context, ct);
};
```

## ⚙️ Configuration

TeleFrame uses Microsoft.Extensions.Configuration for all settings. Configure your bot token through:

### appsettings.json

```json
{
  "TelegramBot": {
    "Token": "YOUR_BOT_TOKEN_HERE"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Environment Variables

```bash
TelegramBot__Token=YOUR_BOT_TOKEN_HERE
```

### Programmatic Configuration

```csharp
var builder = new TelegramBotApplicationBuilder(args);

builder.Configuration["TelegramBot:Token"] = "YOUR_BOT_TOKEN_HERE";
```

## 🔧 Advanced Features

### Custom Handlers with Delegates

Register handlers using simple delegate methods:

```csharp
async Task HandleTextMessage(UpdateContext context, CancellationToken ct)
{
    var text = context.Update.Message!.Text;
    await context.Client.SendTextMessageAsync(
        context.Update.Message.Chat.Id,
        $"Echo: {text}",
        cancellationToken: ct);
}

app.MapMessage(MessageType.Text, HandleTextMessage);
```

### Handler Chaining with Filters

Build complex handler pipelines:

```csharp
var handler = app.MapMessage(m => m.Type == MessageType.Text, async (ctx, ct) =>
{
    await ctx.Client.SendTextMessageAsync(
        ctx.Update.Message!.Chat.Id,
        "Processing...",
        cancellationToken: ct);
})
.Filter<ValidationMiddleware>()
.Filter<AuthenticationMiddleware>();
```

### Dependency Injection

Inject services into your handlers:

```csharp
builder.Services.AddScoped<IMyService, MyService>();

app.MapMessage(MessageType.Text, async (context, ct) =>
{
    var service = context.Services.GetRequiredService<IMyService>();
    var result = await service.ProcessAsync(context.Update.Message!.Text, ct);
    
    await context.Client.SendTextMessageAsync(
        context.Update.Message.Chat.Id,
        result,
        cancellationToken: ct);
});
```

### Async Processing

TeleFrame uses a channel-based system for async update processing:

```csharp
// Configuration happens automatically in TelegramBotApplicationBuilder
// By default, it uses Environment.ProcessorCount * 2 workers
// and Environment.ProcessorCount * 2 * 4 queue capacity
```

## 📚 Examples

### Echo Bot

```csharp
var builder = new TelegramBotApplicationBuilder(args);
var app = builder.Build();

app.MapMessage(m => m.Type == MessageType.Text, async (context, ct) =>
{
    var text = context.Update.Message!.Text;
    await context.Client.SendTextMessageAsync(
        context.Update.Message.Chat.Id,
        $"Echo: {text}",
        cancellationToken: ct);
});

await app.RunAsync();
```

### Interactive Survey Bot

```csharp
var builder = new TelegramBotApplicationBuilder(args);
builder.Services.AddSingleton<IStateManager, MemoryStateManager>();
var app = builder.Build();

// Start survey
app.MapMessage(m => m.Text == "/survey", async (context, ct) =>
{
    var stateManager = context.Services.GetRequiredService<IStateManager>();
    stateManager.SetState("question_1");
    
    await context.Client.SendTextMessageAsync(
        context.Update.Message!.Chat.Id,
        "Q1: What's your favorite color?",
        cancellationToken: ct);
});

// Handle answer 1
app.MapMessage(m => m.Type == MessageType.Text, async (context, ct) =>
{
    var stateManager = context.Services.GetRequiredService<IStateManager>();
    if (stateManager.State == "question_1")
    {
        var answer = context.Update.Message!.Text;
        stateManager.SetState("question_2");
        
        await context.Client.SendTextMessageAsync(
            context.Update.Message.Chat.Id,
            "Q2: What's your favorite food?",
            cancellationToken: ct);
    }
});

await app.RunAsync();
```

### Image Processing Bot

```csharp
var builder = new TelegramBotApplicationBuilder(args);
builder.Services.AddSingleton<IImageProcessor, ImageProcessor>();
var app = builder.Build();

app.MapMessage(MessageType.Photo, async (context, ct) =>
{
    var photo = context.Update.Message!.Photo!.Last();
    var processor = context.Services.GetRequiredService<IImageProcessor>();
    
    // Download and process image
    var fileInfo = await context.Client.GetFileAsync(photo.FileId, cancellationToken: ct);
    var processedImage = await processor.ProcessAsync(fileInfo, ct);
    
    await context.Client.SendPhotoAsync(
        context.Update.Message.Chat.Id,
        new InputFileStream(processedImage, "processed.jpg"),
        caption: "Processed image",
        cancellationToken: ct);
});

await app.RunAsync();
```

## 🛠️ System Requirements

- .NET 10.0 or higher
- C# 12.0 or higher
- A valid Telegram Bot Token (obtain from [@BotFather](https://t.me/botfather))

## 📋 Dependencies

- [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) v22.7.6+
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Caching.Memory
- Microsoft.Extensions.Options.DataAnnotations
- Microsoft.Extensions.Logging

## 📝 License

This project is licensed under the MIT License. See the [LICENCE](LICENCE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## 📞 Support

For issues, questions, or discussions, please visit the [GitHub Issues](https://github.com/M0BIN-V/TeleFrame/issues) page.

---

**Made with ❤️ for Telegram Bot developers**