using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Diagnostic.Tools.Logs
{
    public class LogViewerServiceOptions
    {
        public LogViewerServiceOptions()
        {
            ProcessId = Process.GetCurrentProcess().Id;
        }
        public int ProcessId { get; set; }
    }
}
