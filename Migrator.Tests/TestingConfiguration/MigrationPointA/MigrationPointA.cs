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
            throw new NotImplementedException();
        }
    }

}
