namespace TeleFrame.Tests;

public class UpdateHandlerBuilderTests
{
    [Fact]
    public async Task Build_NoFilters_ReturnsOriginalHandler()
    {
        var called = false;

        var builder = new UpdateHandlerBuilder(Handler);

        var pipeline = builder.Build();

        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var ctx = new UpdateContext(serviceProvider, Substitute.For<ITelegramBotClient>());

        await pipeline(ctx, CancellationToken.None);

        called.ShouldBeTrue();
        return;

        Task Handler(UpdateContext updateContext, CancellationToken cancellationToken)
        {
            called = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Build_WithFilter_InvokesFilterAroundHandler()
    {
        var order = new List<string>();

        var builder = new UpdateHandlerBuilder(Handler);
        builder.Filter(Filter);

        var pipeline = builder.Build();

        var ctx = new UpdateContext(new ServiceCollection().BuildServiceProvider(),
            Substitute.For<ITelegramBotClient>());

        await pipeline(ctx, CancellationToken.None);

        order.ShouldBe([ "before", "handler", "after" ]);
        return;

        UpdateHandlerFilterDelegate Filter(UpdateHandlerFilterDelegate next) =>
            (handlerCtx, ct) =>
            {
                order.Add("before");
                var t = next(handlerCtx, ct);
                order.Add("after");
                return t;
            };

        Task Handler(UpdateContext updateContext, CancellationToken cancellationToken)
        {
            order.Add("handler");
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Filter_T_ResolvesMiddlewareFromServiceProvider()
    {
        var order = new List<string>();

        var middleware = Substitute.For<IUpdateHandlerFilter>();
        middleware
            .When(m => m.InvokeAsync(Arg.Any<UpdateContext>(), Arg.Any<UpdateHandlerFilterDelegate>(),
                Arg.Any<CancellationToken>()))
            .Do(async (ci) =>
            {
                var ctx = ci.ArgAt<UpdateContext>(0);
                var next = ci.ArgAt<UpdateHandlerFilterDelegate>(1);
                var ct = ci.ArgAt<CancellationToken>(2);
                order.Add("before");
                await next(ctx, ct);
                order.Add("after");
            });

        var services = new ServiceCollection();
        services.AddSingleton(middleware);
        var provider = services.BuildServiceProvider();

        var builder = new UpdateHandlerBuilder(Handler);
        builder.Filter<IUpdateHandlerFilter>();

        var pipeline = builder.Build();

        var ctx = new UpdateContext(provider, Substitute.For<ITelegramBotClient>());

        await pipeline(ctx, CancellationToken.None);

        order.ShouldBe(["before", "handler", "after" ]);
        return;

        Task Handler(UpdateContext handlerCtx, CancellationToken ct)
        {
            order.Add("handler");
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Build_IsLazy_EvaluatedOnce()
    {
        var buildCount = 0;

        var builder = new UpdateHandlerBuilder(Handler);

        var pipeline1 = builder.Build();
        var pipeline2 = builder.Build();

        pipeline1.ShouldBeSameAs(pipeline2);

        var ctx = new UpdateContext(new ServiceCollection().BuildServiceProvider(),
            Substitute.For<ITelegramBotClient>());

        await pipeline1(ctx, CancellationToken.None);
        await pipeline2(ctx, CancellationToken.None);

        buildCount.ShouldBe(2);
        return;

        Task Handler(UpdateContext handlerCtx, CancellationToken ct)
        {
            buildCount++;
            return Task.CompletedTask;
        }
    }
}