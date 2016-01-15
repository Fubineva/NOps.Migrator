using System;
using System.Collections.Generic;
using System.Linq;

using NOps.Migrator.MigrationPoints;
using NOps.Migrator.MigrationPoints.Migrations;
using NOps.Migrator.Tests.TestingConfiguration.MigrationPointA;

using NUnit.Framework;

namespace NOps.Migrator.Tests
{
    [TestFixture]
    public class MigrationPointMigratorSequenceTests
    {
        private MigrationPointMigrator _pointMigrator;
        private static int s_sequenceNumber;

        [SetUp]
        public void SetUp()
        {
            _pointMigrator = new MigrationPointMigrator(typeof(MigrationPointA));
            Migration1.Reset();
            Migration2.Reset();
        }
        
        [Test]
        public void Migrate_should_call_migrations_in_same_namespace_as_MigrationPoint_class()
        {
            // arrange
            
            // act
            _pointMigrator.Migrate(0);

            // assert
            Assert.IsTrue(Migration1.Invoked);
            Assert.IsTrue(Migration2.Invoked);
        }

        [Test]
        public void Migrate_should_return_new_versionNumber()
        {
            // arrange

            // act
            var result = _pointMigrator.Migrate(0);

            // assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void Migrate_should_call_migrations_in_order()
        {
            // arrange
            SequenceNumber = 0;
                var pointMigrator = new TestingSequenceMigrationPointMigrator(
                    // passing types out of order (Migration.Number based)
                    new List<Type>()
                    {
                        typeof(Migration2),
                        typeof(Migration1)
                    }
                );
            
            // act
            pointMigrator.Migrate(0);

            // assert
            Assert.IsTrue(Migration1.Invoked);
            Assert.IsTrue(Migration2.Invoked);
            Assert.That(Migration2.SequenceNumber, Is.GreaterThan(Migration1.SequenceNumber));
        }

        [Test]
        public void Migrate_should_tolerate_but_ignore_migrations_without_attributes()
        {
            // arrange
            SequenceNumber = 0;
            var pointMigrator = new TestingSequenceMigrationPointMigrator(
                // passing types out of order (Migration.Number based)
                new List<Type>()
                {
                    typeof(Migration2),
                    typeof(Migration1),
                    typeof(TestingUnattributedMigration)
                }
                );

            // act
            pointMigrator.Migrate(0);

            // assert
            Assert.IsTrue(Migration2.Invoked);
            
            // the TestingUnattributedMigration would throw an exception if Up is invoked.
        }

        [Test]
        public void Migrate_should_skip_migrations_before_current_version()
        {
            // arrange

            // act
            const int current_version = 1;
            _pointMigrator.Migrate(current_version);

            // assert
            Assert.IsFalse(Migration1.Invoked);
            Assert.IsTrue(Migration2.Invoked);
        }

        public static int SequenceNumber
        {
            get
            {
                return ++s_sequenceNumber;
            }
            set
            {
                s_sequenceNumber = value;
            }
        }
    }

    public class TestingUnattributedMigration : IMigrate
    {
        public void Up()
        {
            throw new Exception("This migration should not be invoked, it has no MigrationAttribute.");
        }
    }

    public class TestingSequenceMigrationPointMigrator : MigrationPointMigrator
    {
        private readonly List<Type> _types;

        public TestingSequenceMigrationPointMigrator(IEnumerable<Type> types) : base(typeof(MigrationPointA))
        {
            _types = types.ToList();
        }

        protected override IEnumerable<Type> GetMigrationPointMigrationTypes()
        {

            return _types;
        }
    }
}