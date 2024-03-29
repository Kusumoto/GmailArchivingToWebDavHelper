﻿using GMailArchivingToWebDavHelper.Models;

namespace GMailArchivingToWebDavHelper.Core.Interface;

public interface IMailManager : IDisposable
{
    Task OpenConnection();
    Task<List<MailMessageData>> GetMailMessage();
    Task DeleteMessage(List<MailMessageData> messages);
    Task CloseConnection();
}