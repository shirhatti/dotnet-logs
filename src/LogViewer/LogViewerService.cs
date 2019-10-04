using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tools.RuntimeClient;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogViewer
{
    public class LogViewerService : BackgroundService
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly LogViewerServiceOptions _options;
        private readonly IHostApplicationLifetime _lifetime;
        private static readonly string _MicrosoftExtensionsLoggingProviderName = "Microsoft-Extensions-Logging";
        private readonly IDictionary<string, ILogger> loggerCache = new Dictionary<string, ILogger>();
        public LogViewerService(ILoggerFactory loggerFactory, IOptions<LogViewerServiceOptions> options, IHostApplicationLifetime applicationLifetime)
        {
            _loggerFactory = loggerFactory;
            _options = options.Value;
            _lifetime = applicationLifetime;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var providerList = new List<Provider>()
            {
                new Provider(name: _MicrosoftExtensionsLoggingProviderName,
                             keywords: (ulong)LoggingEventSource.Keywords.FormattedMessage,
                             eventLevel: EventLevel.LogAlways)
            };
            var configuration = new SessionConfigurationV2(
                    circularBufferSizeMB: 1000,
                    format: EventPipeSerializationFormat.NetTrace,
                    requestRundown: false,
                    providers: providerList);

            using var stream = EventPipeClient.CollectTracing2(_options.ProcessId, configuration, out var sessionId);

            cancellationToken.Register(() =>
            {
                EventPipeClient.StopTracing(_options.ProcessId, sessionId);
            });
            
            await Task.Run(() => ProcessEvents(stream));

            // This happens when the process we've attached to has disconnected
            if (!cancellationToken.IsCancellationRequested)
            {
                _lifetime.StopApplication();
            }

        }

        private void ProcessEvents(System.IO.Stream stream)
        {
            using var source = new EventPipeEventSource(stream);
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
    }


}