using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tools.RuntimeClient;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace LogViewer
{
    public class LogViewerService : BackgroundService
    {
        private IDisposable _optionsReloadToken;
        private LoggerFilterOptions _loggerOptions;
        private ulong _sessionId;
        private readonly ILoggerFactory _loggerFactory;
        private readonly LogViewerServiceOptions _logViewerOptions;
        private readonly IHostApplicationLifetime _lifetime;
        private static readonly string _MicrosoftExtensionsLoggingProviderName = "Microsoft-Extensions-Logging";
        private Stream _eventStream;
        private List<Provider> _providerList;
        private SessionConfigurationV2 _configuration;
        private readonly IDictionary<string, ILogger> loggerCache = new Dictionary<string, ILogger>();
        public LogViewerService(ILoggerFactory loggerFactory, IOptions<LogViewerServiceOptions> logViewerOptions, IHostApplicationLifetime applicationLifetime, IOptionsMonitor<LoggerFilterOptions> loggerOptions)
        {
            _loggerOptions = loggerOptions.CurrentValue;
            _optionsReloadToken = loggerOptions.OnChange(ReloadConfiguration);
            _loggerFactory = loggerFactory;
            _logViewerOptions = logViewerOptions.Value;
            _lifetime = applicationLifetime;
        }

        private void ReloadConfiguration(LoggerFilterOptions options)
        {
            _loggerOptions = options;
            _loggerFactory.CreateLogger<LogViewerService>().LogInformation("Configuration was reloaded");
            EventPipeClient.StopTracing(_logViewerOptions.ProcessId, _sessionId);
            BuildConfiguration();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            BuildConfiguration();

            cancellationToken.Register(() =>
            {
                _optionsReloadToken?.Dispose();
                EventPipeClient.StopTracing(_logViewerOptions.ProcessId, _sessionId);
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                _eventStream = EventPipeClient.CollectTracing2(_logViewerOptions.ProcessId, _configuration, out _sessionId);
                await Task.Run(() => ProcessEvents());
            }

            // This happens when the process we've attached to has disconnected
            // TODO: This is might be racy. Review this
            if (!cancellationToken.IsCancellationRequested)
            {
                _lifetime.StopApplication();
            }

        }

        private void BuildConfiguration()
        {
            var filterDataStringBuilder = new StringBuilder();
            filterDataStringBuilder.Append("FilterSpecs=\"");
            foreach (var filter in _loggerOptions.Rules)
            {
                if ((string.IsNullOrEmpty(filter.ProviderName) || filter.ProviderName.Equals(typeof(ConsoleLoggerProvider).FullName)) && filter.LogLevel.HasValue)
                {
                    var categoryName = string.IsNullOrEmpty(filter.CategoryName) ? "Default" : filter.CategoryName;
                    filterDataStringBuilder.Append($"{categoryName}:{filter.LogLevel};");
                }
            }
            filterDataStringBuilder.Append("\"");
            var filterData = filterDataStringBuilder.ToString();
            _providerList = new List<Provider>()
            {
                new Provider(name: _MicrosoftExtensionsLoggingProviderName,
                             keywords: (ulong)LoggingEventSource.Keywords.FormattedMessage,
                             eventLevel: EventLevel.LogAlways,
                             filterData: filterData)
            };
            _configuration = new SessionConfigurationV2(
                    circularBufferSizeMB: 1000,
                    format: EventPipeSerializationFormat.NetTrace,
                    requestRundown: false,
                    providers: _providerList);
        }

        private void ProcessEvents()
        {
            using var source = new EventPipeEventSource(_eventStream);
            source.Dynamic.AddCallbackForProviderEvent(_MicrosoftExtensionsLoggingProviderName, "FormattedMessage", (traceEvent) =>
            {
                // Level, FactoryID, LoggerName, EventID, EventName, FormattedMessage
                var categoryName = (string)traceEvent.PayloadValue(2);
                var logger = _loggerFactory.CreateLogger(categoryName);
                var logLevel = (LogLevel)traceEvent.PayloadValue(0);
                var message = (string)traceEvent.PayloadValue(4);
                logger.Log(logLevel, message);
            });
            source.Process();
        }

        public override void Dispose()
        {
            _optionsReloadToken?.Dispose();
            _eventStream?.Dispose();
        }
    }

    //class MyEventListener : EventListener
    //{
    //    protected override void OnEventSourceCreated(EventSource eventSource)
    //    {
    //        if (eventSource.Name == "Microsoft-Extensions-Logging")
    //        {
    //            // initialize a string, string dictionary of arguments to pass to the EventSource.
    //            // Turn on loggers matching App* to Information, everything else (*) is the default level (which is EventLevel.Error)
    //            var args = new Dictionary<string, string>() { { "FilterSpecs", "App*:Information;*" } };
    //            // Set the default level (verbosity) to Error, and only ask for the formatted messages in this case.
    //            EnableEvents(eventSource, EventLevel.Error, LoggingEventSource.Keywords.FormattedMessage, args);
    //        }
    //    }
    //    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    //    {
    //        // Look for the formatted message event, which has the following argument layout (as defined in the LoggingEventSource.
    //        // FormattedMessage(LogLevel Level, int FactoryID, string LoggerName, string EventId, string FormattedMessage);
    //        if (eventData.EventName == "FormattedMessage")
    //            Console.WriteLine("Logger {0}: {1}", eventData.Payload[2], eventData.Payload[4]);
    //    }
    //}
}