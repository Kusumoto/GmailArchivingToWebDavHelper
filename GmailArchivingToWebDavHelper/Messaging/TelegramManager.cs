using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace GMailArchivingToWebDavHelper.Messaging;

public class TelegramManager : IMessageProviderDelegate
{
    private readonly IConfiguration _configuration;
    private readonly TelegramBotClient _telegramBotClient;

    public TelegramManager(IConfiguration configuration)
    {
        _configuration = configuration;
        _telegramBotClient =
            new TelegramBotClient(configuration.GetSection("Telegram").GetSection("ApiKey").Value ?? "");
    }

    public async Task SendMessage(string message)
    {
        await _telegramBotClient.SendTextMessageAsync(
            _configuration.GetSection("Telegram").GetSection("ChatId").Value ?? "", message);
    }
}