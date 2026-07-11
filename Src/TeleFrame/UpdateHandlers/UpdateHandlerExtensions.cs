namespace TeleFrame.UpdateHandlers;

/// <summary>
///     Provides high-level extension members using C# extension types to seamlessly register conditional
///     update handlers within the execution pipeline of a <see cref="TelegramBotApplication" />.
/// </summary>
public static class UpdateHandlerExtensions
{
    extension(TelegramBotApplication app)
    {
        /// <summary>
        ///     Registers a conditional update handler that executes when the incoming context matches a functional predicate.
        /// </summary>
        /// <param name="predicate">
        ///     The criteria evaluated against the current <see cref="UpdateContext" /> to determine if the
        ///     handler should run.
        /// </param>
        /// <param name="handler">The core delegate execution sequence processed upon a successful predicate match.</param>
        /// <returns>
        ///     An <see cref="UpdateHandlerBuilder" /> instance to allow fluent post-configuration of the registered handler
        ///     pipeline.
        /// </returns>
        /// <remarks>
        ///     If the predicate matches, the pipeline builds and executes the target handler, short-circuiting standard
        ///     middle-tier propagation.
        ///     Otherwise, control flows dynamically to the <c>next</c> middleware delegate.
        /// </remarks>
        public UpdateHandlerBuilder MapUpdate(Func<UpdateContext, bool> predicate, UpdateHandlerDelegate handler)
        {
            var builder = new UpdateHandlerBuilder(handler);

            app.Use(next => async (context, ct) =>
            {
                if (predicate(context))
                {
                    var finalHandler = builder.Build();
                    await finalHandler(context, ct);
                }
                else
                {
                    await next(context, ct);
                }
            });

            return builder;
        }

        /// <summary>
        ///     Maps an open-signature delegate framework to a conditional update routine triggered by a custom predicate
        ///     evaluation.
        /// </summary>
        /// <param name="predicate">
        ///     The evaluation logic applied to the incoming <see cref="UpdateContext" /> to verify route
        ///     compatibility.
        /// </param>
        /// <param name="handler">
        ///     The generic method context compiled dynamically via factory utilities to parse dynamic
        ///     parameters.
        /// </param>
        /// <returns>An <see cref="UpdateHandlerBuilder" /> instance enabling fluent downstream setup behaviors.</returns>
        /// <remarks>
        ///     Internally adapts arbitrary anonymous signatures into a strongly-typed pipeline format using
        ///     <see cref="UpdateHandlerFactory" />.
        /// </remarks>
        public UpdateHandlerBuilder MapUpdate(Func<UpdateContext, bool> predicate, Delegate handler)
        {
            var builder = new UpdateHandlerBuilder(UpdateHandlerFactory.Create(handler));

            app.MapUpdate(predicate, (context, ct) => builder.Build().Invoke(context, ct));

            return builder;
        }

        /// <summary>
        ///     Directs inbound requests smoothly based strictly on the raw underlying <see cref="UpdateType" /> property.
        /// </summary>
        /// <param name="type">
        ///     The targeted Telegram payload category classification (e.g., Message, CallbackQuery, InlineQuery) to
        ///     filter for.
        /// </param>
        /// <param name="handler">The functional block executed upon intercepting matching network payloads.</param>
        /// <returns>An <see cref="UpdateHandlerBuilder" /> infrastructure node reference supporting chained setup behaviors.</returns>
        public UpdateHandlerBuilder MapUpdate(UpdateType type, Delegate handler)
        {
            return app.MapUpdate(c => c.Update.Type == type, handler);
        }
    }
}