using GMailArchivingToWebDavHelper.Core.Interface;
using GMailArchivingToWebDavHelper.Messaging;
using GMailArchivingToWebDavHelper.Models;
using MailKit.Net.Pop3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Net.Mail;

namespace GMailArchivingToWebDavHelper.Core
{
    public class MailManager : IMailManager, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly Pop3Client _pop3Client;
        private readonly IMessageProviderDelegate _messageProvider;

        public MailManager(ILoggerFactory factory, IConfiguration configuration, IMessageProviderDelegate messageProvider)
        {
            _logger = factory.CreateLogger(typeof(MailManager));
            _configuration = configuration;
            _pop3Client = new Pop3Client();
            _messageProvider = messageProvider;
        }

        public virtual async Task OpenConnection()
        {
            var mailServerHost = _configuration.GetSection("MailServer").GetSection("Host").Value;
            var mailServerPort = _configuration.GetSection("MailServer").GetSection("Port").Value ?? "25";
            var mailServerUsername = _configuration.GetSection("MailServer").GetSection("Username").Value;
            var mailServerPassword = _configuration.GetSection("MailServer").GetSection("Password").Value;
            await _pop3Client.ConnectAsync(mailServerHost, int.Parse(mailServerPort), true);
            await _pop3Client.AuthenticateAsync(mailServerUsername, mailServerPassword);
            _logger.LogInformation("Open POP3 connection");
        }

        public virtual async Task<List<MailMessageData>> GetMailMessage()
        {
            var mailList = new List<MailMessageData>();
            _logger.LogInformation($"Total message is {_pop3Client.Count} from POP3 server");
            _logger.LogInformation("Building and getting email datamodel...");
            for (var i = (_pop3Client.Count - 1); i >= 0; i--)
            {
                var message = await _pop3Client.GetMessageAsync(i);
                if (message == null) continue;
                var attachments = new List<AttachmentData>();
                if (message.Attachments.Any())
                {
                    foreach (var file in message.Attachments)
                    {
                        var memoryStream = new MemoryStream();
                        var filename = file.ContentType.Name;
                        if (file is MessagePart messagePart)
                        {
                            await messagePart.Message.WriteToAsync(memoryStream);
                        }
                        else
                        {
                            var part = (MimePart)file;
                            await part.Content.DecodeToAsync(memoryStream);
                        }
                        await memoryStream.FlushAsync();
                        attachments.Add(new AttachmentData()
                        {
                            Filename = filename,
                            DataStream = memoryStream
                        });
                    }
                }
                mailList.Add(new MailMessageData()
                {
                    Index = i,
                    Header = message.Subject,
                    Body = message.TextBody ?? message.HtmlBody,
                    Attachments = attachments,
                    From = message.From.ToString()
                });
            }
            _logger.LogInformation("Build and get email Done!");
            return await Task.FromResult(mailList);
        }

        public virtual async Task DeleteMessage(List<MailMessageData> messages)
        {
            foreach (var mailMessage in messages)
            {
                _logger.LogInformation($"Delete message : {mailMessage.Header}");
                await _messageProvider.SendMessage($"Delete message : {mailMessage.Header}");
                await _pop3Client.DeleteMessageAsync(mailMessage.Index, default);
            }
        }

        public virtual async Task CloseConnection()
        {
            await _pop3Client.DisconnectAsync(true);
            _logger.LogInformation("Close POP3 connection");
        }

        public virtual void Dispose()
        {
            _pop3Client.Dispose();
        }
    }
}
