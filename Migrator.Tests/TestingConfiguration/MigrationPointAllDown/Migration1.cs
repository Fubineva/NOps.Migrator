using Fubineva.NOps.Migrator.MigrationPoints.Migrations;

namespace Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointAllDown
{
    [Migration(1)]
    public class Migration1 : IMigrate, IMigrateDown
    {
        public static bool s_upInvoked = false;
        private static bool s_downInvoked = false;

        public static void Reset()
        {
            s_upInvoked = false;
            s_downInvoked = false;
        }

        public static bool Invoked
        {
            get
            {
                bool tmp = s_upInvoked;
                s_upInvoked = false;
                return tmp;
            }
        }

        public static bool DownInvoked
        {
            get
            {
                var tmp = s_downInvoked;
                s_downInvoked = false;
                return tmp;
            }
        }

        public void Up()
        {
            s_upInvoked = true;
        }

        public void Down()
        {
            s_downInvoked = true;
        }
    }
}
