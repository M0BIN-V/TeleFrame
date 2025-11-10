using System.ComponentModel.DataAnnotations;

namespace TeleFrame.Options;

public class TelegramBotOptions
{
    [Required] [MinLength(10)]
    public required string Token { get; init; }
}