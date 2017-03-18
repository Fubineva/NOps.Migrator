using System;

namespace NOps.Migrator.MigrationPoints.Migrations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MigrationAttribute : Attribute
    {
        private readonly long _number;

        public MigrationAttribute(long number)
        {
            _number = number;
        }

        public long Number
        {
            get
            {
                return _number;
            }
        }
    }
}