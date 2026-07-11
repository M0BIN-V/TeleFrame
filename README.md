# 🚀 TeleFrame

> Build Telegram Bots in .NET with the simplicity of Minimal APIs.

[![NuGet](https://img.shields.io/nuget/v/TeleFrame.svg)]()
[![Downloads](https://img.shields.io/nuget/dt/TeleFrame.svg)]()


TeleFrame is a lightweight Telegram Bot Framework for .NET that brings
Dependency Injection, Middleware, Filters, Routing, and State Management
to Telegram bot development.

Instead of dealing with large update switch statements and repetitive Telegram Bot API boilerplate, TeleFrame lets you define commands, messages, update handlers, middleware, filters, and conversation states using a clean and familiar developer experience.

```csharp
bot.MapCommand("/start", () => "Welcome to TeleFrame!");
```

---

## 📚 Table of Contents

- [📦 Installation](#Installation)
- [🚀 Quick Start](#Quick-Start)
- [💉 Dependency Injection](#Dependency-Injection)
- [⌨️ Commands](#Commands)
    - [Returning Plain Text](#returning-plain-text)
    - [Returning a Telegram Response](#returning-a-telegram-response)
    - [Using UpdateContext](#using-updatecontext)
- [💬 Message Handlers](#Message-Handlers)
- [🎙️ Voice Messages](#voice-messages)
- [🔄 Update Handlers](#update-handlers)
- [🛠️ Middleware](#Middleware)
    - [Register Middleware](#register-middleware)
    - [Inline Middleware](#inline-middleware)
    - [Custom Middleware](#custom-middleware)
- [🛡️ Filters](#filters)
    - [Creating a Filter](#creating-a-filter)
    - [Applying a Filter](#applying-a-filter)
- [🧠 State Management](#state-management)
    - [Set State](#set-state)
    - [Require State](#require-state)
    - [Clear State](#clear-state)
- [📌 UpdateContext](#updatecontext)
- [📨 Results API](#results-api)
- [📋 Logging](#logging)
- [🎯 Example Application](#example-application)
- [🤔 Why TeleFrame?](#why-teleframe)
- [🗺️ Roadmap](#roadmap)
- [🤝 Contributing](#contributing)
- [📄 License](#license)

---

# Installation

Install the package from NuGet:

```bash
dotnet add package TeleFrame
```

---

# Quick Start

Create a bot in just a few lines:

```csharp
var builder = new TelegramBotBuilder(args);

var bot = builder.Build();

bot.MapCommand("/start", () => "Welcome to TeleFrame!");

bot.Run();
```

---


# Dependency Injection

TeleFrame integrates with Microsoft's Dependency Injection container.

Register services:

```csharp
builder.Services.AddScoped<AdminsService>();
```

Inject them directly into handlers:

```csharp
bot.MapCommand("/admins", (AdminsService service) =>
{
    return string.Join(", ", service.Admins);
});
```

No manual service resolution required.

---

# Commands

Commands are the most common way to interact with users.

## Returning Plain Text

```csharp
bot.MapCommand("/start", () =>
{
    return "Welcome to TeleFrame!";
});
```

## Returning a Telegram Response

```csharp
bot.MapCommand("/hi", () =>
{
    return Results.Reply("Hello World");
});
```

## Using UpdateContext

```csharp
bot.MapCommand("/help", (UpdateContext ctx) =>
{
    var username = ctx.Update.Message!.From!.Username;

    return Results.Reply(
        $"Hello {username}",
        messageEffect: MessageEffects.Heart);
});
```

---

# Message Handlers

Handle specific message types.

```csharp
bot.MapMessage(MessageType.Text, () =>
{
    return "Text message received";
});
```

Example:

```csharp
bot.MapMessage(MessageType.Photo, () =>
{
    return "Nice photo!";
});
```

---

# Voice Messages

Handle voice messages with a dedicated API.

```csharp
bot.MapVoice(() =>
{
    return Results.Reply("You sent a voice message!");
});
```

---

# Update Handlers

Handle Telegram updates directly.

Example:

```csharp
bot.MapUpdate(UpdateType.EditedMessage, (UpdateContext ctx) =>
{
    var message = ctx.Update.EditedMessage!;

    return Results.Reply(
        $"You edited: {message.Text}");
});
```

Example for boost notifications:

```csharp
bot.MapUpdate(UpdateType.ChatBoost,
    (UpdateContext ctx, AdminsService service) =>
{
    foreach (var admin in service.Admins)
    {
        ctx.Client.SendMessage(
            admin.Id,
            "Channel boosted");
    }
});
```

---

# Middleware

TeleFrame provides a middleware pipeline similar to ASP.NET Core.

Middleware can inspect, modify, or stop processing updates.

## Register Middleware

```csharp
builder.Services.AddScoped<BlackListMiddleware>();

bot.Use<BlackListMiddleware>();
```

## Inline Middleware

```csharp
bot.Use(next => (context, ct) =>
{
    Console.WriteLine("Update received");

    return next(context, ct);
});
```

---

# Custom Middleware

```csharp
public class BlackListMiddleware : IUpdateMiddleware
{
    public async Task InvokeAsync(
        UpdateContext context,
        UpdateDelegate next,
        CancellationToken ct)
    {
        var blocked = false;

        if (blocked)
            return;

        await next(context, ct);
    }
}
```

---

# Filters

Filters allow validation and authorization before executing handlers.

## Creating a Filter

```csharp
public class OnlyAdminsFilter
    : IUpdateHandlerFilter
{
    public Task InvokeAsync(
        UpdateContext context,
        UpdateHandlerFilterDelegate next,
        CancellationToken ct)
    {
        var isAdmin = true;

        if (isAdmin)
            return next(context, ct);

        return Task.CompletedTask;
    }
}
```

## Applying a Filter

```csharp
bot.MapCommand("/admins", () =>
{
    return "Admin panel";
})
.Filter<OnlyAdminsFilter>();
```

---

# State Management

TeleFrame includes a simple conversation state system.

Perfect for multi-step user interactions.

---

## Set State

```csharp
bot.MapCommand("/verify",
    (IStateManager stateManager) =>
{
    stateManager.SetState(
        "awaiting_phone_number");

    return "Please enter your phone number.";
});
```

---

## Require State

```csharp
bot.MapMessage(MessageType.Text,
    (UpdateContext ctx,
     IStateManager stateManager) =>
{
    var input = ctx.Update.Message!.Text!;

    stateManager.ClearState();

    return "Verification complete.";
})
.RequireState("awaiting_phone_number");
```

---

## Clear State

```csharp
stateManager.ClearState();
```

---

# UpdateContext

`UpdateContext` gives you access to:

* Telegram Client
* Current Update
* User Information
* Chat Information
* Services
* State Management
* Request Context

Example:

```csharp
bot.MapCommand("/me", (UpdateContext ctx) =>
{
    var user = ctx.Update.Message!.From!;

    return Results.Reply(
        $"Hello {user.FirstName}");
});
```

---

# Results API

Return rich responses using the `Results` helper.

```csharp
Results.Reply("Hello");
```

```csharp
Results.Reply(
    "Welcome!",
    messageEffect: MessageEffects.Heart);
```

---

# Logging

Enable update logging with a single line.

Register:

```csharp
builder.Services.AddUpdateLogging();
```

Use:

```csharp
bot.UseUpdateLogging();
```

---

# Example Application

```csharp
var builder = new TelegramBotBuilder(args);

builder.Services.AddScoped<AdminsService>();

var bot = builder.Build();

bot.MapCommand("/start",
    () => "Welcome to TeleFrame!");

bot.MapVoice(
    () => "Voice message received!");

bot.MapCommand("/verify",
    (IStateManager stateManager) =>
{
    stateManager.SetState("awaiting_phone");

    return "Enter your phone number.";
});

bot.Run();
```

---

# Why TeleFrame?

Telegram bot development often starts simple but quickly becomes difficult to maintain as your bot grows.

TeleFrame solves this by bringing proven ASP.NET Core concepts to Telegram bots:

* Routing
* Middleware
* Dependency Injection
* Filters
* State Management
* Clean Architecture Friendly

You focus on business logic.

TeleFrame handles the plumbing.

---

# Roadmap

* [ ] Callback Query Routing
* [ ] Inline Query Support
* [ ] Webhook Hosting
* [ ] Background Jobs
* [ ] Localization Support
* [ ] Built-in Authorization

---

# Contributing

Contributions, ideas, bug reports, and pull requests are welcome.

If you find TeleFrame useful, consider giving the repository a ⭐.

---

# License

Licensed under the MIT License.
