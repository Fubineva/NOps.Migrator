using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Fubineva.NOps.Migrator.MigrationPoints;
using Fubineva.NOps.Migrator.Registry;

using Moq;

using NUnit.Framework;

namespace Fubineva.NOps.Migrator.Tests
{
    [TestFixture]
    public class MigratorTests
    {
        private Migrator _migrator;
        private Mock<IMigrationPointVersionRegistry> _fakeRegistry;
        private List<Type> _migrationPointTypes;

        private const long VERSION_A = 5;
        private const long VERSION_B = 3;

        [SetUp]
        public void Setup()
        {
            _migrationPointTypes = new List<Type>()
                    {
                        typeof(TestingMigrationPointA),
                        typeof(TestingMigrationPointB),
                        typeof(TestingMigrationPointUnregistered)
                    };

            _fakeRegistry = new Mock<IMigrationPointVersionRegistry>();
            _fakeRegistry
                .Setup(r => r.Exists("TestingMigrationPointA"))
                .Returns(true);

            _fakeRegistry
                .Setup(r => r["TestingMigrationPointA"])
                .Returns(new MigrationPointVersion()
                        {
                            Name = "TestingMigrationPointA",
                            Version = VERSION_A
                        })
                .Verifiable("Migrate did not query for TestingMigrationPointA.");

            _fakeRegistry
                .Setup(r => r.Exists("TestingMigrationPointB"))
                .Returns(true);

            _fakeRegistry
                .Setup(r => r["TestingMigrationPointB"])
                .Returns(new MigrationPointVersion()
                        {
                            Name = "TestingMigrationPointB",
                            Version = VERSION_B
                        })
                .Verifiable("Migrate did not query for TestingMigrationPointB.");

            // not in the registry
            _fakeRegistry
                .Setup(r => r.Exists("TestingMigrationPointUnregistered"))
                .Returns(false)
                .Verifiable();

            _migrator = new Migrator(_migrationPointTypes, _fakeRegistry.Object);

        }
        
        [Test]
        public void Migrate_should_query_registry_for_all_provided_types()
        {
            // arrange
            
            // act
            _migrator.Migrate();

            // assert
            _fakeRegistry.Verify();
        }

        [Test]
        public void MigrateMigrationPoint_should_save_registry()
        {
            // arrange
            var migrator = new TestingMigrator(_migrationPointTypes, _fakeRegistry.Object);

            // act
            migrator.Migrate();

            // assert
            _fakeRegistry.Verify(r => r.Save(), Times.Exactly(3));
        }

        [Test]
        public void Migrate_should_invoke_MigrationPointMigrator_with_approriate_versionNumber()
        {
            // arrange
            var migrator = new TestingMigrator(_migrationPointTypes, _fakeRegistry.Object);

            // act
            migrator.Migrate();

            // assert
            Assert.That(migrator.CreatedMigrationPointMigrators["TestingMigrationPointA"].MigrateVersion, Is.EqualTo(VERSION_A));
            Assert.That(migrator.CreatedMigrationPointMigrators["TestingMigrationPointB"].MigrateVersion, Is.EqualTo(VERSION_B));
        }

        [Test]
        public void Migrate_given_unregistered_should_invoke_MigrationPointMigrator_with_version_0()
        {
            // arrange
            var migrator = new TestingMigrator(_migrationPointTypes, _fakeRegistry.Object);

            // act
            migrator.Migrate();

            // assert
            Assert.That(migrator.CreatedMigrationPointMigrators["TestingMigrationPointUnregistered"].MigrateVersion, Is.EqualTo(0));
        }

        [Test]
        public void Migrate_given_unregistered_should_register()
        {
            // arrange
            _fakeRegistry.Setup(r => r.Add("TestingMigrationPointUnregistered", It.Is<long>(n => n == 10))).Verifiable();
            var migrator = new TestingMigrator(_migrationPointTypes, _fakeRegistry.Object);

            // act
            migrator.Migrate();

            // assert
            _fakeRegistry.Verify();
        }

        [Test]
        public void Migrate_running_into_failed_MigrationPoint_migration_should_revert_previous_MigrationPoints_also()
        {
            // arrange
            var migrator = new TestingMigrator(_migrationPointTypes, _fakeRegistry.Object);
            migrator.Fail("TestingMigrationPointUnregistered");
            
            // act
            try
            {
                migrator.Migrate();
            }
            catch(MigrationFailureException)
            {
                // assumed
            }

            // assert
            Assert.That(migrator.CreatedMigrationPointMigrators["TestingMigrationPointA"].RevertInvoked, Is.True);
            Assert.That(migrator.CreatedMigrationPointMigrators["TestingMigrationPointB"].RevertInvoked, Is.True);
        }
        
        [Test]
        [ExpectedException(typeof(RevertedMigrationFailureException))]
        public void Migrate_running_into_failed_MigrationPoint_should_throw()
        {
            // arrange
            var migrator = new TestingMigrator(_migrationPointTypes, _fakeRegistry.Object);
            migrator.Fail("TestingMigrationPointUnregistered");

            migrator.Migrate();

            // assert
        }

        [Test]
        [ExpectedException(typeof(UnrevertedMigrationFailureException))]
        public void Migrate_running_into_failed_MigrationPoint_with_failing_revert_should_throw()
        {
            // arrange
            var migrator = new TestingMigrator(_migrationPointTypes, _fakeRegistry.Object);
            migrator.Fail("TestingMigrationPointUnregistered");
            migrator.FailRevert("TestingMigrationPointA");
            migrator.FailRevert("TestingMigrationPointB");

            try
            {
                migrator.Migrate();
            }
            catch (UnrevertedMigrationFailureException result)
            {
                // assert
                Assert.That(result.InnerException.Message, Is.StringContaining("TestingMigrationPointUnregistered"));
                Assert.NotNull(result.ReversalExceptions.Single(ex => ex.Message.Contains("TestingMigrationPointA")));
                Assert.NotNull(result.ReversalExceptions.Single(ex => ex.Message.Contains("TestingMigrationPointB")));
                throw;
            }
            
            
        }
    }

    public class TestingPassingMigrator : Migrator
    {
        public TestingPassingMigrator(IEnumerable<Type> types, IMigrationPointVersionRegistry migrationPointVersionRegistry) : base(types, migrationPointVersionRegistry)
        {
            
        }
        
        protected override MigrationPointMigrator CreateMigrationPointMigrator(Type migrationPoint)
        {
            return new TestMigrationPointMigrator(migrationPoint);
        }

    }

    public class TestingMigrator : Migrator
    {
        private readonly IDictionary<string, TestMigrationPointMigrator> _createdMigrationPointMigrators = new Dictionary<string, TestMigrationPointMigrator>();
        private string _faiLMigrationPoint;
        private readonly IList<string> _failRevertMigrationPoints = new List<string>();

        public TestingMigrator(IMigrationPointVersionRegistry registry) : base(registry)
        {
        }

        public TestingMigrator(Assembly migrationPointAssembly, IMigrationPointVersionRegistry registry) : base(migrationPointAssembly, registry)
        {
        }

        public TestingMigrator(IEnumerable<Type> migrationPointTypes, IMigrationPointVersionRegistry registry) : base(migrationPointTypes, registry)
        {
        }

        public IDictionary<string, TestMigrationPointMigrator> CreatedMigrationPointMigrators
        {
            get
            {
                return _createdMigrationPointMigrators;
            }
        }

        protected override MigrationPointMigrator CreateMigrationPointMigrator(Type migrationPoint)
        {
            var testMigrationPointMigrator = new TestMigrationPointMigrator(migrationPoint);
            CreatedMigrationPointMigrators.Add(migrationPoint.Name, testMigrationPointMigrator);

            if (migrationPoint.Name == _faiLMigrationPoint)
            {
                testMigrationPointMigrator.Fail();
                _faiLMigrationPoint = null;
            }
            if (_failRevertMigrationPoints.Contains(migrationPoint.Name))
            {
                testMigrationPointMigrator.FailRevert();
                _failRevertMigrationPoints.Remove(migrationPoint.Name);
            }
            return testMigrationPointMigrator;
        }

        public void Fail(string failMigrationPoint)
        {
            _faiLMigrationPoint = failMigrationPoint;
        }

        public void FailRevert(string failRevertMigrationPoint)
        {
            _failRevertMigrationPoints.Add(failRevertMigrationPoint);
        }
    }

    public class TestMigrationPointMigrator : MigrationPointMigrator
    {
        private long _migrateVersion;
        private bool _fail;
        private bool _revertInvoked;
        private bool _failRevert;

        public TestMigrationPointMigrator(Type migrationPoint) : base(migrationPoint)
        {
        }

        public long MigrateVersion
        {
            get
            {
                return _migrateVersion;
            }
        }

        public bool RevertInvoked
        {
            get
            {
                return _revertInvoked;
            }
        }

        public override void Revert()
        {
            _revertInvoked = true;
            if (_failRevert) throw new UnrevertedMigrationFailureException(MigrationPointName + " reversal failed!");
        }

        public override long Migrate(long currentVersion)
        {
            if (_fail) throw new MigrationFailureException(MigrationPointName + " failed migration! ");
            _migrateVersion = currentVersion;
            return 10;
        }

        public void Fail()
        {
            _fail = true;
        }

        public void FailRevert()
        {
            _failRevert = true;
        }
    }

    public class TestingMigrationPointUnregistered : IMigrationPoint
    {
        public void Backup(string label)
        {
            throw new NotImplementedException();
        }

        public void Restore(string label)
        {
            throw new NotImplementedException();
        }
    }

    public class TestingMigrationPointA : IMigrationPoint
    {
        public void Backup(string label)
        {
            throw new NotImplementedException();
        }

        public void Restore(string label)
        {
            throw new NotImplementedException();
        }
    }

    public class TestingMigrationPointB : IMigrationPoint
    {
        public void Backup(string label)
        {
            throw new NotImplementedException();
        }

        public void Restore(string label)
        {
            throw new NotImplementedException();
        }
    }
}