using System;

namespace NOps.Migrator.MigrationPoints
{
    public interface IMigrationPoint
    {
        void Backup(string label);

        void Restore(string label);

        void OnMigrationStarting();

        void OnMigrationCompleted();

        void OnMigrationFailed(long failingMigrationNumber, Exception exception);
    }
}