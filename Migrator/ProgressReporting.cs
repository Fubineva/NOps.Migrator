using System.Diagnostics;

namespace Fubineva.NOps.Migrator
{
    public delegate void ProgressReport(string progressMessage);

    public static class ProgressReporting
    {
        [DebuggerStepThrough]
        public static void NullMessageReceiver(string message)
        {
            Trace.WriteLine(message);
        }
    }
}