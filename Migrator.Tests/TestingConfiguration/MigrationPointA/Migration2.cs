using Fubineva.NOps.Migrator.MigrationPoints.Migrations;

namespace Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointA
{
    [Migration(2)]
    public class Migration2 : IMigrate 
    {
        public static bool s_invoked = false;
        private static int s_sequenceNumber = -1;

        public static void Reset()
        {
            s_invoked = false;
            s_sequenceNumber = -1;
        }

        public static bool Invoked
        {
            get
            {
                bool tmp = s_invoked;
                s_invoked = false;
                return tmp;
            }
            
        }

        public static int SequenceNumber
        {
            get
            {
                return s_sequenceNumber;
            }
            
        }

        public void Up()
        {
            s_invoked = true;
            s_sequenceNumber = MigrationPointMigratorSequenceTests.SequenceNumber;
        }
    }
}
