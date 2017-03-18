using NOps.Migrator.MigrationPoints.Migrations;

namespace NOps.Migrator.Tests.TestingConfiguration.MigrationPointANonDown
{
    [Migration(1)]
    public class Migration1 : IMigrate, IMigrateDown
    {
        public static bool s_invoked = false;
        
        public static void Reset()
        {
            s_invoked = false;
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

        public void Up()
        {
            s_invoked = true;
        }

        public void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}
