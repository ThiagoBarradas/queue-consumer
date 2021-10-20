using Serilog;
using Serilog.Builder;
using Serilog.Builder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QueueConsumer.Models
{
    public class QueueConsumerConfiguration
    {
        public QueueConsumerConfiguration()
        {
            this.QueueConnectionString = Environment.GetEnvironmentVariable("QueueConnectionString");
            this.QueueName = Environment.GetEnvironmentVariable("QueueName");
            this.Url = Environment.GetEnvironmentVariable("Url");
            this.User = Environment.GetEnvironmentVariable("User");
            this.Pass = Environment.GetEnvironmentVariable("Pass");
            this.AuthToken = Environment.GetEnvironmentVariable("AuthToken");
            this.TimeoutInSeconds = int.Parse(Environment.GetEnvironmentVariable("TimeoutInSeconds") ?? "60");
            this.MaxThreads = int.Parse(Environment.GetEnvironmentVariable("MaxThreads") ?? "20");
            this.PopulateQueueQuantity = int.Parse(Environment.GetEnvironmentVariable("PopulateQueueQuantity") ?? "0");
            this.CreateQueue = bool.Parse(Environment.GetEnvironmentVariable("CreateQueue") ?? "false");
            this.RetryTTL = int.Parse(Environment.GetEnvironmentVariable("RetryTTL") ?? "60000");
            this.RetryCount = int.Parse(Environment.GetEnvironmentVariable("RetryCount") ?? "5");
            this.Condition = Environment.GetEnvironmentVariable("Condition");
            this.StatusCodeAcceptToSuccess = Environment.GetEnvironmentVariable("StatusCodeAcceptToSuccess") ?? "200;201;202;204";
            this.StatusCodeAcceptToSuccessList = null;
            this.LogDomain = Environment.GetEnvironmentVariable("LogDomain");
            this.LogApplication = Environment.GetEnvironmentVariable("LogApplication");
            this.LogBlacklist = Environment.GetEnvironmentVariable("LogBlacklist");
            this.LogDebugEnabled = bool.Parse(Environment.GetEnvironmentVariable("LogDebugEnabled") ?? "false");
            this.LogSeqEnabled = bool.Parse(Environment.GetEnvironmentVariable("LogSeqEnabled") ?? "false");
            this.LogSeqUrl = Environment.GetEnvironmentVariable("LogSeqUrl");
            this.LogSeqApiKey = Environment.GetEnvironmentVariable("LogSeqApiKey");
            this.LogSplunkEnabled = bool.Parse(Environment.GetEnvironmentVariable("LogSplunkEnabled") ?? "false"); 
            this.LogSplunkUrl = Environment.GetEnvironmentVariable("LogSplunkUrl");
            this.LogSplunkToken = Environment.GetEnvironmentVariable("LogSplunkToken");
            this.LogSplunkIndex = Environment.GetEnvironmentVariable("LogSplunkIndex");
            this.LogSplunkCompany = Environment.GetEnvironmentVariable("LogSplunkCompany");
            this.LogNewRelicEnabled = bool.Parse(Environment.GetEnvironmentVariable("LogNewRelicEnabled") ?? "false");
            this.LogNewRelicAppName = Environment.GetEnvironmentVariable("NEW_RELIC_APP_NAME");
            this.LogNewRelicLicenseKey = Environment.GetEnvironmentVariable("NEW_RELIC_LICENSE_KEY");
            this.NewRelicApmEnabled = Environment.GetEnvironmentVariable("CORECLR_ENABLE_PROFILING") == "1";

            this.SetupLogger();
        }

        public void SetupLogger()
        {
            this.LogEnabled = (this.LogSeqEnabled || this.LogSplunkEnabled || this.LogNewRelicEnabled);

            if (!this.LogEnabled)
            {
                return;
            }

            var loggerBuilder = new LoggerBuilder().UseSuggestedSetting(this.LogDomain, this.LogApplication);

            if (this.LogSeqEnabled)
            {
                loggerBuilder.EnableSeq(this.LogSeqUrl, this.LogSeqApiKey);
            }

            if (this.LogSplunkEnabled)
            {
                loggerBuilder.SetupSplunk(new SplunkOptions
                {
                    Enabled = true,
                    Token = this.LogSplunkToken,
                    Url = this.LogSplunkUrl,
                    Index = this.LogSplunkIndex,
                    Company = this.LogSplunkCompany,
                    ProcessName = $"{this.LogSplunkCompany}.{this.LogDomain}.{this.LogApplication}",
                    SourceType = "_json",
                    ProductVersion = "1.0.0"
                });
            }

            if (this.LogNewRelicEnabled)
            {
                loggerBuilder.EnableNewRelic(this.LogNewRelicAppName, this.LogNewRelicLicenseKey);
            }

            if (this.LogDebugEnabled)
            {
                loggerBuilder.EnableDebug();
            }

            Log.Logger = loggerBuilder.BuildLogger();
        }

        public int RetryCount { get; set; }

        public bool CreateQueue { get; set; }

        public int RetryTTL { get; set; }

        public int PopulateQueueQuantity { get; set; }

        public int MaxThreads { get; set; }

        public string QueueConnectionString { get; set; }

        public string Condition { get; set; }

        public string QueueName { get; set; }

        public string Url { get; set; }

        public string User { get; set; }

        public string Pass { get; set; }

        public string AuthToken { get; set; }

        public string StatusCodeAcceptToSuccess { get; set; }

        public string LogDomain { get; set; }

        public string LogApplication { get; set; }

        public List<string> LogBlacklistList { get; set; } = new List<string>();

        public string LogBlacklist 
        { 
            get
            {
                return string.Join(",", this.LogBlacklistList);
            }
            set
            {
                if (value == null)
                {
                    this.LogBlacklistList = new List<string>();
                }

                this.LogBlacklistList = this.LogBlacklist.Split(",").ToList();
            }
        }

        public bool LogEnabled { get; set; }

        public bool LogDebugEnabled { get; set; }

        public bool LogSeqEnabled { get; set; }

        public string LogSeqUrl { get; set; }

        public string LogSeqApiKey { get; set; }

        public bool LogSplunkEnabled { get; set; }

        public string LogSplunkUrl { get; set; }

        public string LogSplunkToken { get; set; }

        public string LogSplunkIndex { get; set; }

        public string LogSplunkCompany { get; set; }

        public bool LogNewRelicEnabled { get; set; }

        public string LogNewRelicAppName { get; set; }

        public string LogNewRelicLicenseKey { get; set; }

        public bool NewRelicApmEnabled { get; set; }

        public IList<int> _statusCodeAcceptToSuccessList { get; set; }

        public IList<int> StatusCodeAcceptToSuccessList
        {
            get => _statusCodeAcceptToSuccessList;
            private set
            {
                this._statusCodeAcceptToSuccessList = this.StatusCodeAcceptToSuccess?.Split(";").Select(int.Parse).ToList();
            }
        }

        public int TimeoutInSeconds { get; set; }

        public static QueueConsumerConfiguration Create()
        {
            return new QueueConsumerConfiguration();
        }

        public static QueueConsumerConfiguration CreateForDebug(bool populate)
        {
            return new QueueConsumerConfiguration
            {
                AuthToken = "token",
                CreateQueue = true,
                MaxThreads = 100,
                Pass = "pass",
                PopulateQueueQuantity = populate ? 100000 : 0,
                QueueConnectionString = "amqp://guest:guest@localhost:5672/",
                QueueName = "debug",
                RetryCount = 5,
                RetryTTL = 30000,
                TimeoutInSeconds = 30,
                Url = "http://pruu.herokuapp.com/dump/queue-consumer",
                User = "user",
                StatusCodeAcceptToSuccess = "200;201;202;204",
                StatusCodeAcceptToSuccessList = null,
            };
        }
    }
}