using Fubineva.NOps.Migrator.MigrationPoints;
using Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointANonDown;
using Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointAllDown;

using NUnit.Framework;

using Migration1 = Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointANonDown.Migration1;
using Migration2 = Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointANonDown.Migration2;

namespace Fubineva.NOps.Migrator.Tests
{
    [TestFixture]
    public class MigrationPointMigratorPreMigrationTests
    {
        [SetUp]
        public void SetUp()
        {
            Migration1.Reset();
            Migration2.Reset();
            
            TestingConfiguration.MigrationPointAllDown.Migration1.Reset();
            TestingConfiguration.MigrationPointAllDown.Migration2.Reset();
            
            MigrationPointANonDown.Reset();
            MigrationPointAllDown.Reset();
        }

        [Test]
        public void Migrate_given_non_down_migration_should_invoke_backup()
        {
            // arrange
            var migrator = new MigrationPointMigrator(typeof(MigrationPointANonDown));
            Migration2.FailUp = false;
            MigrationPointANonDown.FailBackup = false;

            // act
            migrator.Migrate(0);

            // assert
            Assert.IsTrue(MigrationPointANonDown.BackupInvoked, "No backup call was made.");
        }

        [Test]
        public void Migrate_given_all_down_migrations_should_not_invoke_backup()
        {
            // arrange
            var migrator = new MigrationPointMigrator(typeof(MigrationPointAllDown));
            TestingConfiguration.MigrationPointAllDown.Migration2.FailUp = false;
            
            // act
            migrator.Migrate(0);

            // assert
            Assert.IsFalse(MigrationPointAllDown.BackupInvoked, "a backup call was made where it shouldn't have.");
        }
    }

}