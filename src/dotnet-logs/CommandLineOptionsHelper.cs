using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace Microsoft.Diagnostics.Tools.Logs
{
    internal static class CommandLineOptionsHelper
    {
        public static Option ProcessIdOption()
        {
            var option = new Option("--process-id");
            option.AddAlias("-p");
            option.Argument = new Argument<int>()
            {
                Arity = ArgumentArity.ExactlyOne,
            };

            return option;
        }
    }
}
