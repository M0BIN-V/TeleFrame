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
        if (returnType == typeof(void))
            return Expression.Block(call, Expression.Property(null, typeof(Task), nameof(Task.CompletedTask)));

        if (returnType == typeof(Task)) return call;

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerType = returnType.GetGenericArguments()[0];
            MethodInfo sendAsyncMethod;

            if (innerType == typeof(string))
                sendAsyncMethod = typeof(Extensions)
                    .GetMethod(nameof(Extensions.SendTextAsync), BindingFlags.Static | BindingFlags.Public)!;
            else if (typeof(ITelegramResult).IsAssignableFrom(innerType))
                sendAsyncMethod = typeof(Extensions)
                    .GetMethod(nameof(Extensions.SendResultAsync), BindingFlags.Static | BindingFlags.Public)!;
            else
                throw new InvalidOperationException($"Return type Task<{innerType.Name}> is not supported.");

            var taskParam = Expression.Parameter(returnType, "t");
            var sendCall = Expression.Call(sendAsyncMethod, contextParam, Expression.Property(taskParam, "Result"));
            var contLambda = Expression.Lambda(sendCall, taskParam);

            return Expression.Call(call, nameof(Task.ContinueWith), Type.EmptyTypes, contLambda);
        }

        if (returnType == typeof(string))
        {
            var sendAsync = typeof(Extensions)
                .GetMethod(nameof(Extensions.SendTextAsync), BindingFlags.Static | BindingFlags.Public)!;
            return Expression.Call(sendAsync, contextParam, call);
        }

        if (typeof(ITelegramResult).IsAssignableFrom(returnType) &&
            returnType is { IsAbstract: false, IsInterface: false })
        {
            var sendAsync = typeof(Extensions)
                .GetMethod(nameof(Extensions.SendResultAsync), BindingFlags.Static | BindingFlags.Public)!;
            return Expression.Call(sendAsync, contextParam, call);
        }

        throw new InvalidOperationException($"Return type {returnType.Name} is not supported.");
    }
}

public static class Extensions
{
    public static Task SendTextAsync(UpdateContext context, string text)
    {
        return new TextResult(text).InvokeAsync(context);
    }

    public static Task SendResultAsync(UpdateContext context, ITelegramResult result)
    {
        return result.InvokeAsync(context);
    }
}