using System.CommandLine;
using System.CommandLine.Invocation;

namespace Microsoft.Diagnostics.Tools.Logs.Commands
{
    public static class ListProcessesCommand
    {
        public static Command Create() =>
            new Command(
                name: "ps",
                description: "Lists dotnet processes that can be attached to.")
            {
                Handler = CommandHandler.Create<IConsole>(ProcessStatusCommandHandler.PrintProcessStatus)
            };
    }
}
