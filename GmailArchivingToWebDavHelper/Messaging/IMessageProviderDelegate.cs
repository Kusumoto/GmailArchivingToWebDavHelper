namespace GMailArchivingToWebDavHelper.Messaging
{
    public interface IMessageProviderDelegate
    {
        Task SendMessage(string message);
    }
}
