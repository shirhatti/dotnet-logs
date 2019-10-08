using Microsoft.Diagnostics.Tools.Logs.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Microsoft.Diagnostics.Tools.Logs.Commands
{
    public static class MonitorCommand
    {
        public static Command Create()
        {
            var command = new Command(
                name: "monitor",
                description: "Attach to specified process and tail logs")
            {
                Handler = CommandHandler.Create<int>(async (processId) =>
                {
                    await Host.CreateDefaultBuilder()
                    .ConfigureLogging((context, logging) =>
                    {
                        logging.Services.RemoveAll<ILoggerProvider>();
                    })
                    .ConfigureServices((hostBuilder, services) =>
                    {
                        services.AddHostedService<LogViewerService>();
                        services.Configure<LogViewerServiceOptions>(o => o.ProcessId = processId);
                        services.Configure<LogViewerServiceOptions>(hostBuilder.Configuration);
                    })
                    .Build()
                    .RunAsync();
                })
            };

            command.AddOption(ProcessIdOption.Create());

            return command;
        }
    }
}
