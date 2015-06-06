using System;

using Fubineva.NOps.Migrator.MigrationPoints.Migrations;

namespace Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointANonDown
{
    [Migration(2)]
    public class Migration2 : IMigrate 
    {
        public static bool s_invoked = false;
        private static bool s_failUp;

        public static void Reset()
        {
            s_invoked = false;
            s_failUp = false;
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

        public static bool FailUp
        {
            set
            {
                s_failUp = value;
            }
        }

        public void Up()
        {
            s_invoked = true;
            if (s_failUp) throw new Exception("Failing migration!");
        }
    }
}
