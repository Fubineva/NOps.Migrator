using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace NOps.Migrator
{
    public delegate void ProgressReport(string progressMessage);
    
    public class ProgressReporting
    {
        public static ILogger s_logger;

        static ProgressReporting()
        {
            // ToDo: review if we want to keep this, added this just cause we lost System.Diagnostics.Trace
            ILoggerFactory loggerFactory = new LoggerFactory();
            s_logger = loggerFactory.CreateLogger<ProgressReporting>();
        }

        [DebuggerStepThrough]
        public static void NullMessageReceiver(string message)
        {
            s_logger.LogTrace(message);
        }
    }
}