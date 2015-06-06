using System;

using Fubineva.NOps.Migrator.MigrationPoints;

namespace Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointANonDown
{
    public class MigrationPointANonDown : IMigrationPoint
    {
        private static bool s_backupInvoked;
        private static bool s_backupRestored;
        private static bool s_failBackup;
        private static bool s_failRestore;

        public static void Reset()
        {
            s_backupInvoked = false;
            s_backupRestored = false;
            s_failBackup = false;
            s_failRestore = false;
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

        public static bool FailBackup
        {
            set
            {
                s_failBackup = value;
            }
        }

        public static bool FailRestore
        {
            set
            {
                s_failRestore = value;
            }
        }

        public void Backup(string label)
        {
            s_backupInvoked = true;
            if (s_failBackup) throw new Exception("Backup failed!");
        }

        public void Restore(string label)
        {
            s_backupRestored = true;
            if (s_failRestore) throw new Exception("Restore failed!");
        }

    }
}
