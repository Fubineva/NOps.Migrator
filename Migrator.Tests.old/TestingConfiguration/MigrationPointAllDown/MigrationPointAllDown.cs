using System;

using NOps.Migrator.MigrationPoints;

namespace NOps.Migrator.Tests.TestingConfiguration.MigrationPointAllDown
{
    public class MigrationPointAllDown : IMigrationPoint
    {
        private static bool s_backupInvoked;
        private static bool s_backupRestored;

        public static void Reset()
        {
            s_backupInvoked = false;
            s_backupRestored = false;
        }

        public static bool BackupInvoked
        {
            get
            {
                bool tmp = s_backupInvoked;
                s_backupInvoked = false;
                return tmp;
            }
        }

        public static bool BackupRestored
        {
            get
            {
                bool tmp = s_backupRestored;
                s_backupRestored = false;
                return tmp;
            }
        }

        public void Backup(string label)
        {
            s_backupInvoked = true;
        }

        public void Restore(string label)
        {
            s_backupRestored = true;
        }

        public void OnMigrationStarting()
        {
            ;
        }

        public void OnMigrationCompleted()
        {
            ;
        }

        public void OnMigrationFailed(long failingMigrationNumber, Exception exception)
        {
            ;
        }
    }
}
