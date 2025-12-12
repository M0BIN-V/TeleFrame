namespace TeleFrame.Application;

public class ProcessingOptions
{
    public required int WorkerCount { get; set; }
    public required int QueueCapacity { get; set; }
}