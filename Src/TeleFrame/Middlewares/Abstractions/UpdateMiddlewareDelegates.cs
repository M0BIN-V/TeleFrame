namespace TeleFrame.Middlewares.Abstractions;

public delegate Task UpdateMiddlewareDelegate(UpdateContext context, CancellationToken cancellationToken);

public delegate UpdateMiddlewareDelegate UpdateMiddleware(UpdateMiddlewareDelegate next);