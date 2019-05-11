using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tools.RuntimeClient;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogViewer
{
    public class LogViewerService : HostedService
    {
        private readonly ILogger _logger;
        private readonly LogViewerServiceOptions _options;
        private readonly IApplicationLifetime _lifetime;
        public LogViewerService(ILogger<LogViewerService> logger, IOptions<LogViewerServiceOptions> options, IApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _options = options.Value;
            _lifetime = applicationLifetime;
        }
        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var providerList = new List<Provider>()
            {
                new Provider(name: "Microsoft-Extensions-Logging",
                             eventLevel: System.Diagnostics.Tracing.EventLevel.Warning)
            };
            var configuration = new SessionConfiguration(
                    circularBufferSizeMB: 1000,
                    outputPath: "",
                    providers: providerList);

            var binaryReader = EventPipeClient.CollectTracing(_options.ProcessId, configuration, out var sessionId);
            var source = new EventPipeEventSource(binaryReader);
            source.Dynamic.All += EventSourceHandler;
            source.Process();
            _lifetime.StopApplication();
            return Task.CompletedTask;
        }
        private void EventSourceHandler(TraceEvent traceEvent)
        {
            _logger.LogInformation(traceEvent.EventName);
        }
    }
}