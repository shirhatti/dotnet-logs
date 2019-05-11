using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace Microsoft.Extensions.Logging
{
    public static class LoggingEventSource
    {
        public enum Keywords
        {
            Meta = 1,
            Message = 2,
            FormattedMessage = 4,
            JsonMessage = 8
        }
    }
}
