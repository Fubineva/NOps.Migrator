using System.Runtime.CompilerServices;

namespace NOps.Migrator.Registry
{
    public interface IMigrationPointVersionRegistry
    {
        [IndexerName("MigrationPoint")]
        MigrationPointVersion this[string migrationPointName] { get; }

        void Add(string migrationPointName, long version);
        bool Exists(string migrationPointName);
        void Save();
    }
}