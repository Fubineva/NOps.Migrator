using System.Xml.Serialization;

namespace NOps.Migrator.Registry
{
    public class MigrationPointVersion
    {
        internal MigrationPointVersion()
        {
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public long Version { get; set; }

    }
}