using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using TeleFrame.Results;

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
        var instance = handler.Target != null ? Expression.Constant(handler.Target) : null;
        var call = Expression.Call(instance, method, args);

        var body = WrapReturnValue(call, method.ReturnType, contextParam);

        var lambda = Expression.Lambda<UpdateHandlerDelegate>(body, contextParam, ctParam);
        return lambda.Compile();
    }

    static List<Expression> BuildArguments(ParameterInfo[] parameters, ParameterExpression contextParam,
        ParameterExpression ctParam)
    {
        var args = new List<Expression>(parameters.Length);

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
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m =>
                m is
                {
                    Name: nameof(ServiceProviderServiceExtensions.GetRequiredService),
                    IsGenericMethodDefinition: true
                } &&
                m.GetParameters().Length == 1)
            .MakeGenericMethod(param.ParameterType);

        return Expression.Call(getService, servicesProp);
    }

    static Expression WrapReturnValue(Expression call, Type returnType, ParameterExpression contextParam)
    {
        // void
        if (returnType == typeof(void))
            return Expression.Block(call, Expression.Constant(Task.CompletedTask));

        // Task
        if (returnType == typeof(Task))
            return call;

        // Task<T>
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerType = returnType.GetGenericArguments()[0];

            if (innerType == typeof(string))
            {
                var sendAsync = typeof(Extensions).GetMethod(nameof(Extensions.SendTextTaskAsync))!;
                return Expression.Call(sendAsync, contextParam, call);
            }

            if (typeof(ITelegramResult).IsAssignableFrom(innerType))
            {
                var sendAsync = typeof(Extensions).GetMethod(nameof(Extensions.SendResultTaskAsync))!;
                return Expression.Call(sendAsync, contextParam, call);
            }
        }

        // string
        if (returnType == typeof(string))
        {
            var sendAsync = typeof(Extensions).GetMethod(nameof(Extensions.SendTextAsync))!;
            return Expression.Call(sendAsync, contextParam, call);
        }

        // ITelegramResult
        if (typeof(ITelegramResult).IsAssignableFrom(returnType) && !returnType.IsInterface && !returnType.IsAbstract)
        {
            var sendAsync = typeof(Extensions).GetMethod(nameof(Extensions.SendResultAsync))!;
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

    public static async Task SendTextTaskAsync(UpdateContext context, Task<string> text)
    {
        var awaited = await text;
        await new TextResult(awaited).InvokeAsync(context);
    }

    public static async Task SendResultAsync(UpdateContext context, ITelegramResult result)
    {
        await result.InvokeAsync(context);
    }

    public static async Task SendResultTaskAsync(UpdateContext context, Task<ITelegramResult> result)
    {
        var awaited = await result;
        await awaited.InvokeAsync(context);
    }
}