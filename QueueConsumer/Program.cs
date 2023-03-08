using QueueConsumer.Models;
using QueueConsumer.Notification;

using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace QueueConsumer;

public class Program
{
    public static void Main()
    {
        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        try
        {
            var tokenSource = new CancellationTokenSource();
            var config = QueueConsumerConfiguration.Create();

            var queueConsumerJwt = new QueueConsumerJwt(config);
            var sendNotificationClient = new SendNotificationClient(config, queueConsumerJwt);
            
            DisplayHeader(config);
            
            var processor = new QueueMessageProcessor(config, sendNotificationClient);

            var task = new Task(() =>
            {
                while (!processor.Execute()) { }
            }, tokenSource.Token);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("Aborting Program...");
                tokenSource.Cancel();
                task.Dispose();
            };

            task.Start();
            task.Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine("Program Exception:");
            Console.WriteLine(" - {0}\n\n{1}", e.Message, e.StackTrace);
        }
    }

    private static void DisplayHeader(QueueConsumerConfiguration config)
    {
        Logger.LogLineWithLevel("INFO", "Queue Consumer Application Started");
        Logger.LogLine("");
        Logger.LogLine("Configuration:");
        Logger.LogLine("- QueueConnectionString: {0}", Regex.Replace(config.QueueConnectionString, "(\\:\\/\\/).*(\\@)", "://*****@"));
        Logger.LogLine("- QueueName: {0}", config.QueueName);
        Logger.LogLine("- Url: {0}", config.Url);
        Logger.LogLine("- User: {0}", config.User);
        Logger.LogLine("- Pass: {0}", string.IsNullOrWhiteSpace(config.Pass) ? "null" : "******");
        Logger.LogLine("- AuthToken: {0}", string.IsNullOrWhiteSpace(config.AuthToken) ? "null" : "******");
        Logger.LogLine("- AuthUrl: {0}", string.IsNullOrWhiteSpace(config.AuthUrl));
        Logger.LogLine("- ClientId: {0}", string.IsNullOrWhiteSpace(config.ClientId) ? "null" : "******");
        Logger.LogLine("- ClientSecret: {0}", string.IsNullOrWhiteSpace(config.ClientSecret) ? "null" : "******");
        Logger.LogLine("- UserAgent: {0}", config.UserAgent);
        Logger.LogLine("- TimeoutInSeconds: {0}", config.TimeoutInSeconds);
        Logger.LogLine("- MaxThreads: {0}", config.MaxThreads);
        Logger.LogLine("- PopulateQueueQuantity: {0}", config.PopulateQueueQuantity);
        Logger.LogLine("- CreateQueue: {0}", config.CreateQueue);
        Logger.LogLine("- RetryCount: {0}", config.RetryCount);
        Logger.LogLine("- RetryTTL: {0}", config.RetryTTL);
        Logger.LogLine("- Condition: {0}", config.Condition);
        Logger.LogLine("- StatusCodeAcceptToSuccessList: {0}", string.Join(";", config.StatusCodeAcceptToSuccessList));

        Logger.LogLine("- LogEnabled: {0}", config.LogEnabled);
        Logger.LogLine("- LogDomain: {0}", config.LogDomain);
        Logger.LogLine("- LogApplication: {0}", config.LogApplication);
        Logger.LogLine("- LogBlacklist: {0}", config.LogBlacklist);

        Logger.LogLine("- LogSeqEnabled: {0}", config.LogSeqEnabled);
        Logger.LogLine("- LogSeqUrl: {0}", config.LogSeqUrl);
        Logger.LogLine("- LogSeqApiKey: {0}", string.IsNullOrWhiteSpace(config.LogSeqApiKey) ? "null" : "******");

        Logger.LogLine("- LogSplunkEnabled: {0}", config.LogSplunkEnabled);
        Logger.LogLine("- LogSplunkUrl: {0}", config.LogSplunkUrl);
        Logger.LogLine("- LogSplunkIndex: {0}", config.LogSplunkIndex);
        Logger.LogLine("- LogSplunkCompany: {0}", config.LogSplunkCompany);
        Logger.LogLine("- LogSplunkToken: {0}", string.IsNullOrWhiteSpace(config.LogSplunkToken) ? "null" : "******");

        Logger.LogLine("- LogNewRelicEnabled: {0}", config.LogNewRelicEnabled);
        Logger.LogLine("- LogNewRelicAppName: {0}", config.LogNewRelicAppName);
        Logger.LogLine("- LogNewRelicLicenseKey: {0}", string.IsNullOrWhiteSpace(config.LogNewRelicLicenseKey) ? "null" : "******");
        Logger.LogLine("- NewRelicApmEnabled: {0}", config.NewRelicApmEnabled);

        Logger.LogLine("");
    }
}

public static class Logger
{
    public static void LogLineWithLevel(string logLevel, string message, params object[] args)
    {
        var finalMessage = $"[{GetCurrentDate()}][{logLevel}] {message ?? ""}";
        Console.WriteLine(finalMessage, args);
    }

    public static void LogWithLevel(string logLevel, string message, params object[] args)
    {
        var finalMessage = $"[{GetCurrentDate()}][{logLevel}] {message ?? ""}";
        Console.Write(finalMessage, args);
    }

    public static void LogLine(string message, params object[] args)
    {
        Console.WriteLine(message, args);
    }

    public static void Log(string message, params object[] args)
    {
        Console.Write(message, args);
    }

    private static string GetCurrentDate()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }
}