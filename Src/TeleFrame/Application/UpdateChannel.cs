using System.Threading.Channels;

namespace TeleFrame.Application;

internal class UpdateChannel
{
    readonly Channel<Update> _channel;

    public UpdateChannel(int capacity = 200)
    {
        _channel = Channel.CreateBounded<Update>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    }

    public ChannelWriter<Update> Writer => _channel.Writer;
    public IAsyncEnumerable<Update> Reader => _channel.Reader.ReadAllAsync();
}