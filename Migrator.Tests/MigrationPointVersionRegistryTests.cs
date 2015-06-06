using System;
using System.IO;
using System.Reflection;

using Fubineva.NOps.Migrator.Registry;
using Fubineva.NOps.Migrator.Tests.Helpers;

using NUnit.Framework;

namespace Fubineva.NOps.Migrator.Tests
{
    [TestFixture]
    public class MigrationPointVersionRegistryTests
    {
        [Test]
        public void Save_should_write_a_file()
        {
            // arrange
            var dvReg = new MigrationPointVersionRegistry();
            dvReg.Add("MyDb", 5);
            
            // act
            using (var fileLoc = TempFileLocation.Create())
            {
                var registryFilePathName = fileLoc.File("MigrationPointVersionRegistry.cfg");
                dvReg.Save(registryFilePathName);
                
                // assert
                Assert.IsTrue(File.Exists(registryFilePathName));
                Assert.AreEqual(
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<MigrationPointVersions xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n  <MigrationPointVersion Name=\"MyDb\" Version=\"5\" />\r\n</MigrationPointVersions>", 
                    File.ReadAllText(registryFilePathName)
                );
            }
        }

        [Test]
        public void Load()
        {
            // arrange
            var filePathName = Path.Combine(TestDataPath(), "MigrationPointVersionRegistry.cfg");
            
            // act
            var result = MigrationPointVersionRegistry.Load(filePathName);

            // assert
            Assert.That(result.Exists("MyDbA"));
            Assert.That(result.Exists("MyDbB"));
        }

        private string TestDataPath()
        {
            return Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath),"TestData");
        }
    }
}
