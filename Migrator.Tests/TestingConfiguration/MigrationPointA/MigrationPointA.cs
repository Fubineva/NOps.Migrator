using System;

using Fubineva.NOps.Migrator.MigrationPoints;

namespace Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointA
{
    public class MigrationPointA : IMigrationPoint
    {
        public void Backup(string label)
        {
            
        }

        public void Restore(string label)
        {
            ;
        }

        public void OnMigrationStarting()
        {
            StartingInvoked = true;
        }

        public void OnMigrationCompleted()
        {
            CompletedInvoked = true;
        }

        public void OnMigrationFailed(long failingMigrationNumber, Exception exception)
        {
            FailedInvoked = true;
        }

        internal static bool StartingInvoked { get; private set; }

        internal static bool CompletedInvoked { get; private set; }

        internal static bool FailedInvoked { get; private set; }

        internal static void Reset()
        {
            StartingInvoked = false;
            CompletedInvoked = false;
            FailedInvoked = false;
        }
    }

}
