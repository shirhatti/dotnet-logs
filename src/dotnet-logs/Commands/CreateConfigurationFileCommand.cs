using Microsoft.Diagnostics.Tools.Logs.Options;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;

namespace Microsoft.Diagnostics.Tools.Logs.Commands
{
    public static class CreateConfigurationFileCommand
    {
        public static readonly string _configurationFileContents = @"
{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft"": ""Warning""
    }
  }
}";

        public static Command Create()
        {
            var command = new Command(
                name: "create-config",
                description: "Creates a templated configuration file in the current directory")
            {
                Handler = CommandHandler.Create<IConsole, bool>(CreateConfigurationFile)
            };

            command.AddOption(ForceOption.Create());

            return command;
        }

        private static void CreateConfigurationFile(IConsole console, bool ForceOption)
        {
            if (File.Exists("appsettings.json") && !ForceOption)
            {
                console.Out.WriteLine("appsettings.json already exists. Use --force to overwrite");
            }
            else
            {
                using var stream = File.CreateText("appsettings.json");
                stream.Write(_configurationFileContents);
                stream.Close();
            }
        }
    }
}
