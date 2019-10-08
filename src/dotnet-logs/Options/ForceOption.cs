using System.CommandLine;

namespace Microsoft.Diagnostics.Tools.Logs.Options
{
    public static class ForceOption
    {
        public static Option Create()
        {
            var option = new Option("--force");
            option.AddAlias("-f");
            option.Argument = new Argument<bool>()
            {
                Arity = ArgumentArity.Zero
            };         
            return option;
        }
    }
}
