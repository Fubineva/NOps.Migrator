namespace Fubineva.NOps.Migrator.MigrationPoints
{
    public interface IMigrationPoint
    {
        void Backup(string label);

        void Restore(string label);
    }
}