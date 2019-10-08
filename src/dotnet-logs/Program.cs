using Microsoft.Diagnostics.Tools.Logs.Commands;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Tools.Logs
{
    public class Program
    {
        public async static Task<int> Main(string[] args) =>
            await new CommandLineBuilder()
                .AddCommand(ListProcessesCommand.Create())
                .AddCommand(MonitorCommand.Create())
                .AddCommand(CreateConfigurationFileCommand.Create())
                .UseDefaults()
                .Build()
                .InvokeAsync(args);

    }
}
