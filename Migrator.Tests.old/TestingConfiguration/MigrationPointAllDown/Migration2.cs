using System;

using NOps.Migrator.MigrationPoints.Migrations;

namespace NOps.Migrator.Tests.TestingConfiguration.MigrationPointAllDown
{
    [Migration(2)]
    public class Migration2 : IMigrate, IMigrateDown
    {
        private static bool s_upInvoked = false;
        private static bool s_failUp;
        private static bool s_downInvoked = false;

        public static void Reset()
        {
            s_upInvoked = false;
            s_failUp = false;
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

        public static bool FailUp
        {
            set
            {
                s_failUp = value;
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
            if (s_failUp) throw new Exception("Failing migration!");
        }

        public void Down()
        {
            s_downInvoked = true;
        }
    }
}
