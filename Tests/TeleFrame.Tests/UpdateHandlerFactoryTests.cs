using TeleFrame.Results;

namespace TeleFrame.Tests;

public class UpdateHandlerFactoryTests
{
    [Fact]
    public async Task Create_WithVoidHandler_CallsHandler()
    {
        var called = false;

        var del = Handler;

        var pipeline = UpdateHandlerFactory.Create(del);

        var ctx = new UpdateContext(new ServiceCollection().BuildServiceProvider(),
            Substitute.For<ITelegramBotClient>());
        await pipeline(ctx, CancellationToken.None);

        called.ShouldBeTrue();
        return;

        void Handler(UpdateContext handlerCtx)
        {
            called = true;
        }
    }

    [Fact]
    public async Task Create_WithTaskReturningHandler_CallsHandler()
    {
        var called = false;

        var del = Handler;

        var pipeline = UpdateHandlerFactory.Create(del);

        var ctx = new UpdateContext(new ServiceCollection().BuildServiceProvider(),
            Substitute.For<ITelegramBotClient>());
        await pipeline(ctx, CancellationToken.None);

        called.ShouldBeTrue();
        return;

        Task Handler(UpdateContext handlerCtx)
        {
            called = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Create_WithStringReturningHandler_SendsText()
    {
        string Handler(UpdateContext ctx)
        {
            return "hello";
        }

        var del = Handler;

        var client = Substitute.For<ITelegramBotClient>();

        var svc = new ServiceCollection().BuildServiceProvider();
        var ctx = new UpdateContext(svc, client)
        {
            Update = new Update
            {
                Message = new Message
                {
                    Chat = new Chat { Id = 123 }
                }
            }
        };

        var pipeline = UpdateHandlerFactory.Create(del);

        await pipeline(ctx, CancellationToken.None);

        await client.Received(1)
            .SendRequest(Arg.Is<SendMessageRequest>(r => r.Text == "hello"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WithTaskOfStringReturningHandler_SendsTextAfterAwait()
    {
        var del = Handler;

        var client = Substitute.For<ITelegramBotClient>();

        var svc = new ServiceCollection().BuildServiceProvider();
        var ctx = new UpdateContext(svc, client)
        {
            Update = new Update
            {
                Message = new Message
                {
                    Chat = new Chat
                    {
                        Id = 321
                    }
                }
            }
        };

        var pipeline = UpdateHandlerFactory.Create(del);

        await pipeline(ctx, CancellationToken.None);

        await client.Received(1)
            .SendRequest(Arg.Is<SendMessageRequest>(r => r.Text == "world"), Arg.Any<CancellationToken>());
        return;

        async Task<string> Handler(UpdateContext handlerCtx)
        {
            await Task.Yield();
            return "world";
        }
    }

    [Fact]
    public async Task Create_WithITelegramResult_ReturnsResultInvoked()
    {
        var del = Handler;

        var client = Substitute.For<ITelegramBotClient>();
        var ctx = new UpdateContext(new ServiceCollection().BuildServiceProvider(), client)
        {
            Update = new Update { Message = new Message { Chat = new Chat { Id = 111 } } }
        };

        var pipeline = UpdateHandlerFactory.Create(del);

        await pipeline(ctx, CancellationToken.None);

        await client.Received(1)
            .SendRequest(Arg.Is<SendMessageRequest>(r => r.Text == "res"), Arg.Any<CancellationToken>());
        return;

        TestResult Handler(UpdateContext handlerCtx)
        {
            return new TestResult("res");
        }
    }

    [Fact]
    public async Task Create_ResolvesServices_ForNonContextParameters()
    {
        var service = Substitute.For<IService>();
        var services = new ServiceCollection();
        services.AddSingleton(service);
        var provider = services.BuildServiceProvider();

        var del = Handler;

        var pipeline = UpdateHandlerFactory.Create(del);

        var ctx = new UpdateContext(provider, Substitute.For<ITelegramBotClient>());

        await pipeline(ctx, CancellationToken.None);

        service.Received(1).DoSomething();
        return;

        Task Handler(UpdateContext handlerCtx, IService s)
        {
            s.DoSomething();
            return Task.CompletedTask;
        }
    }

    class TestResult : ITelegramResult
    {
        readonly string _text;

        public TestResult(string text)
        {
            _text = text;
        }

        public Task InvokeAsync(UpdateContext ctx)
        {
            return ctx.Client.SendRequest(new SendMessageRequest
                {
                    Text = _text,
                    ChatId = ctx.Update.Message?.Chat.Id ?? throw new NullReferenceException()
                },
                CancellationToken.None);
        }
    }

    public interface IService
    {
        void DoSomething();
    }
}