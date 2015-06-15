using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Fubineva.NOps.Migrator.MigrationPoints;
using Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointA;
using Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointANonDown;

using NUnit.Framework;

using Migration1 = Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointA.Migration1;
using Migration2 = Fubineva.NOps.Migrator.Tests.TestingConfiguration.MigrationPointA.Migration2;

namespace Fubineva.NOps.Migrator.Tests
{
    public class MigrationPointMigratorEventsTests
    {
        [SetUp]
        public void SetUp()
        {
            Migration1.Reset();
            Migration2.Reset();

            MigrationPointA.Reset();
            
        }


        [Test]
        public void Migrate_should_invoke_OnMigrationStarting()
        {
            // arrange
            var migrator = new MigrationPointMigrator(typeof(MigrationPointA));

            // act
            migrator.Migrate(0);

            // assert
            Assert.IsTrue(MigrationPointA.StartingInvoked, "No migration starting event.");
        }

        [Test]
        public void Migrate_should_invoke_OnMigrationCompleted()
        {
            // arrange
            var migrator = new MigrationPointMigrator(typeof(MigrationPointA));

            // act
            migrator.Migrate(0);

            // assert
            Assert.IsTrue(MigrationPointA.StartingInvoked, "No migration completed event.");
        }

        [Test]
        public void Migrate_should_invoke_OnFailure()
        {

            // arrange
            var migrator = new MigrationPointMigrator(typeof(MigrationPointANonDown));
            TestingConfiguration.MigrationPointANonDown.Migration2.FailUp = true;

            // act
            try
            {
                migrator.Migrate(0);
            }
            catch (MigrationFailureException)
            {
                ; //ignore these
            }

            Assert.IsTrue(MigrationPointANonDown.FailureEventInvoked);
            Assert.AreEqual(2, MigrationPointANonDown.FailureEvent.MigrationNumber);
            Assert.AreEqual("Failing migration!", MigrationPointANonDown.FailureEvent.Exception.Message);
            // assert

        }
    }
}
