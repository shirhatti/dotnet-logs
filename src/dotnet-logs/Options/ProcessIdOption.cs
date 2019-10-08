using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace Microsoft.Diagnostics.Tools.Logs.Options
{
    internal static class ProcessIdOption
    {
        public static Option Create()
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
