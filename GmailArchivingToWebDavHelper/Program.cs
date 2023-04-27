using GMailArchivingToWebDavHelper;
using GMailArchivingToWebDavHelper.Core;
using GMailArchivingToWebDavHelper.Core.Interface;
using GMailArchivingToWebDavHelper.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

var hostBuilder = CreateHostBuilder(args).Build();
var config = hostBuilder.Services.GetService<IConfiguration>();
if (config?.GetSection("EnableQuartz").Value is not "True")
    await hostBuilder.Services.GetRequiredService<Worker>().StartAsync();
else
    await hostBuilder.RunAsync();

static IConfigurationRoot ConfigurationBuilder()
{
    return new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true)
        .AddEnvironmentVariables()
        .Build();
}

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureLogging(config =>
        {
            config.ClearProviders();
            config.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
            });
        })
        .ConfigureServices((hostBuilder, services) =>
        {
            services.AddSingleton<IConfiguration>(ConfigurationBuilder());
            services.AddTransient<IFileManager, FileManager>();
            services.AddTransient<IMailManager, MailManager>();
            services.AddTransient<DiscordManager>();
            services.AddTransient<TelegramManager>();
            services.AddTransient<LineManager>();
            services.AddTransient<IMessageProviderDelegate>(serviceProvider =>
            {
                var msgProvider = hostBuilder.Configuration.GetSection("MessageDriver").Value;
                return (msgProvider switch
                {
                    MessageProvider.Discord => serviceProvider.GetService<DiscordManager>(),
                    MessageProvider.Line => serviceProvider.GetService<LineManager>(),
                    MessageProvider.Telegram => serviceProvider.GetService<TelegramManager>(),
                    _ => throw new NotImplementedException("Provider not implement")
                })!;
            });
            services.AddTransient<Worker>();

            if (hostBuilder.Configuration.GetSection("EnableQuartz").Value is not "True") return;
            var quartzTime = hostBuilder.Configuration.GetSection("QuartzTime").Value;
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                var jobKey = new JobKey("JobWorker");
                q.AddJob<JobWorker>(jobKey);
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithCronSchedule(quartzTime ?? "0 0 1 * * ?"));
            });
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        });
}