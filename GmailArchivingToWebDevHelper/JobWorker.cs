using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMailArchivingToWebDavHelper.Messaging;
using Microsoft.Extensions.Logging;
using Quartz;

namespace GMailArchivingToWebDavHelper
{
    public class JobWorker : IJob
    {
        private readonly Worker _worker;
        private readonly ILogger _logger;
        private readonly IMessageProviderDelegate _messageProvider;
        public JobWorker(ILoggerFactory factory, Worker worker, IMessageProviderDelegate messageProvider)
        {
            _worker = worker;
            _messageProvider = messageProvider;
            _logger = factory.CreateLogger(typeof(JobWorker));
        }
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Start job at : {context.FireTimeUtc}");
            await _messageProvider.SendMessage($"Start job at : {context.FireTimeUtc}");
            await _worker.StartAsync();
            _logger.LogInformation($"This job will be executed again at: {context.NextFireTimeUtc}");
            await _messageProvider.SendMessage($"This job will be executed again at: {context.NextFireTimeUtc}");
        }
    }
}
