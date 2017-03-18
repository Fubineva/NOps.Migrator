using NOps.Migrator.MigrationPoints;
using NOps.Migrator.Tests.TestingConfiguration.MigrationPointAllDown;
using NOps.Migrator.Tests.TestingConfiguration.MigrationPointANonDown;

using NUnit.Framework;

using Migration1 = NOps.Migrator.Tests.TestingConfiguration.MigrationPointANonDown.Migration1;
using Migration2 = NOps.Migrator.Tests.TestingConfiguration.MigrationPointANonDown.Migration2;

namespace NOps.Migrator.Tests
{
    [TestFixture]
    public class MigrationPointMigratorFailureTests
    {
        [SetUp]
        public void Setup()
        {
            Migration1.Reset();
            Migration2.Reset();

            TestingConfiguration.MigrationPointAllDown.Migration1.Reset();
            TestingConfiguration.MigrationPointAllDown.Migration2.Reset();

            MigrationPointANonDown.Reset();
            MigrationPointAllDown.Reset();
        }

        [Test]
        public void Migrate_given_a_failing_sequence_should_throw()
        {
            // arrange
            var migrator = new MigrationPointMigrator(typeof(MigrationPointANonDown));
            Migration2.FailUp = true;

            // act
            Assert.Throws<RevertedMigrationFailureException>(() => migrator.Migrate(0));
        
            // assert
        }

        [Test]
        public void Migrate_given_a_non_revertable_sequence_and_failing_backup_should_throw()
        {
            // arrange
            var migrator = new MigrationPointMigrator(typeof(MigrationPointANonDown));
            MigrationPointANonDown.FailBackup = true;

            // act
            Assert.Throws<BackupFailureException>(() => migrator.Migrate(0));

            // assert
        }


        [Test]
        public void Migrate_given_a_failing_non_down_migration_should_invoke_restore()
        {
            // arrange
            var migrator = new MigrationPointMigrator(typeof(MigrationPointANonDown));
            Migration2.FailUp = true;

            // act
            try 
            { 
                migrator.Migrate(0);
            }
            catch (RevertedMigrationFailureException)
            {
                // assumed
            }

            // assert
            Assert.IsTrue(MigrationPointANonDown.BackupRestored, "No restore call was made.");
        }

        [Test]
        public void Migrate_given_a_failing_all_down_migration_should_reverse()
        {
            // arrange
            var migrator = new MigrationPointMigrator(typeof(MigrationPointAllDown));
            TestingConfiguration.MigrationPointAllDown.Migration2.FailUp = true;

            // act
            try
            {
                migrator.Migrate(0);
            }
            catch (RevertedMigrationFailureException)
            {
                // assumed
            }

            // assert
            Assert.IsFalse(MigrationPointANonDown.BackupRestored, "A restore call was made.");
            Assert.That(TestingConfiguration.MigrationPointAllDown.Migration1.DownInvoked, Is.True);
            Assert.That(TestingConfiguration.MigrationPointAllDown.Migration2.DownInvoked, Is.False);
        }


        [Test]
        public void Migrate_given_all_down_migrations_should_not_invoke_backup()
        {
            // arrange
            var migrator = new MigrationPointMigrator(typeof(MigrationPointAllDown));

            // act
            try
            {
                migrator.Migrate(0);
            }
            catch(RevertedMigrationFailureException)
            {
                // assumed
            }
            
            // assert
            Assert.IsFalse(MigrationPointAllDown.BackupInvoked, "a backup call was made where it shouldn't have.");
        }

    }

}