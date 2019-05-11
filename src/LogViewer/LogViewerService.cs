using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tools.RuntimeClient;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogViewer
{
    public class LogViewerService : HostedService
    {
        private readonly ILogger _logger;
        private readonly LogViewerServiceOptions _options;
        public LogViewerService(ILogger<LogViewerService> logger, IOptions<LogViewerServiceOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }
        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var providerList = new List<Provider>()
            {
                new Provider(name: "System.Runtime",
                             eventLevel: System.Diagnostics.Tracing.EventLevel.Informational)
            };
            var configuration = new SessionConfiguration(
                    circularBufferSizeMB: 1000,
                    outputPath: "",
                    providers: providerList);

            var binaryReader = EventPipeClient.CollectTracing(/*_options.ProcessId*/26232, configuration, out var sessionId);
            var source = new EventPipeEventSource(binaryReader);
            source.Dynamic.All += EventSourceHandler;
            source.Process();
            return Task.CompletedTask;
        }
        private void EventSourceHandler(TraceEvent traceEvent)
        {
            if (traceEvent.EventName.Equals("EventCounters"))
            {
                _logger.LogInformation("Counter received");
            }
        }
    }
}