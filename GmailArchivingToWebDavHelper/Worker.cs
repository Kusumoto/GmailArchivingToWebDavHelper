﻿using GMailArchivingToWebDavHelper.Core.Interface;
using GMailArchivingToWebDavHelper.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using GMailArchivingToWebDavHelper.Messaging;

namespace GMailArchivingToWebDavHelper
{
    public class Worker
    {
        private readonly IFileManager _fileManager;
        private readonly IMailManager _mailManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IMessageProviderDelegate _messageProvider;

        public Worker(ILoggerFactory factory, IFileManager fileManager, IMailManager mailManager, IConfiguration configuration, IMessageProviderDelegate messageProvider)
        {
            _fileManager = fileManager;
            _mailManager = mailManager;
            _configuration = configuration;
            _messageProvider = messageProvider;
            _logger = factory.CreateLogger(typeof(Worker));
        }

        public async Task StartAsync()
        {
            await _messageProvider.SendMessage("Bot starting ...");
            await _mailManager.OpenConnection();
            var messageMoveList = new List<MailMessageData>();
            var messagesList = await _mailManager.GetMailMessage();
            var messagesWithAttachmentList = messagesList.Where(f => f.Attachments.Any()).ToList();
            var filterSettingList = _configuration.GetSection("FilterSettings").Get<List<FilterSettingData>>();

            foreach (var mailMessageData in messagesWithAttachmentList)
            {
                var filter = filterSettingList?.FirstOrDefault(f => IsMatchFromFilterSetting(f, mailMessageData));
                
                if (filter is null) continue;
                await _messageProvider.SendMessage($"Email match in condition | Subject : {mailMessageData.Header}");
                _logger.LogInformation($"Email match in condition | Subject : {mailMessageData.Header}");

                var attachments = mailMessageData.Attachments
                    .Where(mailMessageAttachment => string.IsNullOrEmpty(filter.FileFormatFilter) || new Regex(filter.FileFormatFilter).IsMatch(mailMessageAttachment.Filename));

                foreach (var mailMessageAttachment in attachments)
                {
                    await _fileManager.UploadFileToWebDav(mailMessageAttachment.Filename, filter.FilePath,
                        mailMessageAttachment.DataStream);
                }

                messageMoveList.Add(mailMessageData);
            }

            await _mailManager.DeleteMessage(messageMoveList);
            await _mailManager.CloseConnection();
        }

        private static bool IsMatchFromFilterSetting(FilterSettingData filterSettingData,
            MailMessageData mailMessageData) => new Regex(filterSettingData.HeaderRegEx).IsMatch(mailMessageData.Header) 
                                                && mailMessageData.From.Contains(filterSettingData.EmailFrom)
                                                && (!string.IsNullOrEmpty(filterSettingData.BodyRegEx) && new Regex(filterSettingData.BodyRegEx).IsMatch(mailMessageData.Body));
    }
}
