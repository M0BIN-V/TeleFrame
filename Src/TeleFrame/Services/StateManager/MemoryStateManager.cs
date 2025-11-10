using Microsoft.Extensions.Caching.Memory;

namespace TeleFrame.Services.StateManager;

internal class MemoryStateManager : IStateManager
{
    readonly IMemoryCache _cache;
    readonly UpdateContext _context;

    public MemoryStateManager(UpdateContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
        State = _cache.GetOrCreate(GetChatId(), _ => string.Empty)!;
    }

    public string State { get; }

    public void SetState(string state)
    {
        _cache.Set(GetChatId(), state);
    }

    long GetChatId()
    {
        return _context.Update.Message?.Chat.Id ??
               throw new NullReferenceException("Chat id is null");
    }
}