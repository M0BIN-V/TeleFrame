using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TeleFrame.Results;
using TeleFrame.Services;

namespace TeleFrame.UpdateHandlers;

public static class UpdateHandlerFactory
{
    static readonly ConcurrentDictionary<MethodInfo, UpdateHandlerDelegate> Cache = new();

    public static UpdateHandlerDelegate Create(Delegate handler)
    {
        return Cache.GetOrAdd(handler.Method, _ => BuildDelegate(handler));
    }

    static UpdateHandlerDelegate BuildDelegate(Delegate handler)
    {
        var method = handler.Method;
        var parameters = method.GetParameters();

        var contextParam = Expression.Parameter(typeof(UpdateContext), "context");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

        var args = BuildArguments(parameters, contextParam, ctParam);

        var instance = handler.Target is not null ? Expression.Constant(handler.Target) : null;
        var call = Expression.Call(instance, method, args);

        var body = WrapReturnValue(call, method.ReturnType, contextParam);

        var lambda = Expression.Lambda<UpdateHandlerDelegate>(body, contextParam, ctParam);
        return lambda.Compile();
    }

    static List<Expression> BuildArguments(ParameterInfo[] parameters, ParameterExpression contextParam,
        ParameterExpression ctParam)
    {
        var args = new List<Expression>();

        foreach (var param in parameters)
            if (param.ParameterType == typeof(UpdateContext))
                args.Add(contextParam);
            else if (param.ParameterType == typeof(CancellationToken))
                args.Add(ctParam);
            else
                args.Add(BuildServiceResolveExpression(param, contextParam));

        return args;
    }

    static MethodCallExpression BuildServiceResolveExpression(ParameterInfo param, ParameterExpression contextParam)
    {
        var servicesProp = Expression.Property(contextParam, nameof(UpdateContext.Services));

        var getService = typeof(ServiceProviderServiceExtensions)
            .GetMethods()
            .Single(m => m is
            {
                Name: nameof(ServiceProviderServiceExtensions.GetRequiredService),
                IsGenericMethodDefinition: true
            } && m.GetParameters().Length == 1)
            .MakeGenericMethod(param.ParameterType);

        return Expression.Call(getService, servicesProp);
    }

    static Expression WrapReturnValue(Expression call, Type returnType, ParameterExpression contextParam)
    {
        if (typeof(Task).IsAssignableFrom(returnType))
            return call;

        if (returnType == typeof(string))
        {
            var sendAsync = typeof(Extensions)
                .GetMethod(nameof(Extensions.SendTextAsync), BindingFlags.Static | BindingFlags.Public)!;
            var sendCall = Expression.Call(sendAsync, contextParam, call);
            return sendCall;
        }

        if (returnType.IsAssignableTo(typeof(ITelegramResult)) &&
            returnType is
            {
                IsAbstract: false,
                IsInterface: false
            })
        {
            var sendAsync = typeof(Extensions)
                .GetMethod(nameof(Extensions.SendResultAsync), BindingFlags.Static | BindingFlags.Public)!;
            var sendCall = Expression.Call(sendAsync, contextParam, call);
            return sendCall;
        }

        var completed = Expression.Property(null, typeof(Task), nameof(Task.CompletedTask));
        return Expression.Block(call, completed);
    }
}

public static class Extensions
{
    public static Task SendTextAsync(UpdateContext context, string text)
    {
        var chatId = context.Update.Message?.Chat.Id ?? 0;
        return context.Services.GetRequiredService<ITelegramBotClient>().SendMessage(chatId, text);
    }

    public static Task SendResultAsync(UpdateContext context, ITelegramResult result)
    {
        return result.InvokeAsync(context);
    }
}