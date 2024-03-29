﻿using System.Net.Http.Headers;
using System.Text;
using GMailArchivingToWebDavHelper.Core.Interface;
using GMailArchivingToWebDavHelper.Exceptions;
using GMailArchivingToWebDavHelper.Messaging;
using GMailArchivingToWebDavHelper.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace GMailArchivingToWebDavHelper.Core;

public class FileManager : IFileManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IMessageProviderDelegate _messageProvider;

    public FileManager(ILoggerFactory factory, IConfiguration configuration, IMessageProviderDelegate messageProvider)
    {
        _configuration = configuration;
        _logger = factory.CreateLogger(typeof(FileManager));
        _messageProvider = messageProvider;
    }

    public virtual async Task UploadFileToWebDav(string filename, FilterSettingData filterSetting, MemoryStream stream)
    {
        var basePath = _configuration.GetSection("WebDev").GetSection("BasePath").Value ?? "";
        var username = _configuration.GetSection("WebDev").GetSection("Username").Value ?? "";
        var password = _configuration.GetSection("WebDev").GetSection("Password").Value ?? "";
        var prefixPathWithDateFormat = DateTime.Now.ToString(filterSetting.PrefixFilename);
        var uploadPath = string.Concat(basePath, filterSetting.FilePath, "/", prefixPathWithDateFormat, "-", filename);


        using var httpClient = new HttpClient();
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, uploadPath);
        requestMessage.Content = new ByteArrayContent(stream.ToArray());
        requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeTypes.GetMimeType(filename));
        requestMessage.Content.Headers.ContentLength = stream.Length;

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));

        await _messageProvider.SendMessage($"Uploading file {requestMessage.RequestUri?.AbsoluteUri} ...");
        _logger.LogInformation($"Uploading file {requestMessage.RequestUri?.AbsoluteUri} ...");

        using var response = await httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode)
        {
            await _messageProvider.SendMessage(
                $"Error upload file {requestMessage.RequestUri?.AbsoluteUri} | {response.StatusCode}");
            _logger.LogError($"Error upload file {requestMessage.RequestUri?.AbsoluteUri} | {response.StatusCode}");
            throw new AppException($"Cannot upload file {filterSetting.FilePath}/{filename}");
        }

        await _messageProvider.SendMessage($"Upload file {requestMessage.RequestUri?.AbsoluteUri} success!");
        _logger.LogInformation($"Upload file {requestMessage.RequestUri?.AbsoluteUri} success!");
    }
}