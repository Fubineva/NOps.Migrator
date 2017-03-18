using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;

namespace NOps.Migrator.Registry
{
	public class MigrationPointVersionRegistry : IMigrationPointVersionRegistry
	{
		[XmlRoot("MigrationPointVersions")]
		public class MigrationPointVersionCollection : KeyedCollection<string, MigrationPointVersion>
		{
			protected override string GetKeyForItem(MigrationPointVersion item)
			{
				return item.Name;
			}
		}

		private readonly MigrationPointVersionCollection _migrationPointVersions = new MigrationPointVersionCollection();
		private string _filePathName;

		public MigrationPointVersionRegistry()
		{
			
		}

		private MigrationPointVersionRegistry(MigrationPointVersionCollection migrationPointVersionCollection)
		{
			_migrationPointVersions = migrationPointVersionCollection;
		}

		[IndexerName("MigrationPoint")]
		public MigrationPointVersion this[string migrationPointName]
		{
			get
			{
				if (_migrationPointVersions.Contains(migrationPointName))
				{
					return _migrationPointVersions[migrationPointName];
				}
				
				return null;
			}
		}
		
		public MigrationPointVersion Item(string migrationPointName)
		{
			return this[migrationPointName];
		}

		public void Add(string migrationPointName, long version)
		{
			var t = new MigrationPointVersion
					{
						Name = migrationPointName,
						Version = version,
					};
 
			_migrationPointVersions.Add(t);
		}

		public static MigrationPointVersionRegistry Load(string filePathName)
		{
			MigrationPointVersionCollection migrationPointVersionCollection;
			using (var fileStream = new FileStream(filePathName, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var configReader = XmlReader.Create(fileStream))
			{
				var deserializer = new XmlSerializer(typeof(MigrationPointVersionCollection));
				try
				{
					migrationPointVersionCollection = (MigrationPointVersionCollection)deserializer.Deserialize(configReader);
				}
				catch (XmlException ex)
				{
					throw new Exception(string.Format("The configuration file {0} contains invalid Xml.", filePathName), ex);
				}
			}

			return new MigrationPointVersionRegistry(migrationPointVersionCollection) { FilePathName = filePathName};
		}

		public string FilePathName
		{
			get
			{
				return _filePathName;
			}
			set
			{
				_filePathName = value;
			}
		}

		public void Save(string filePathName)
		{
			var xmlWriterSettings = new XmlWriterSettings
			{
				Indent = true,
			};

			using (var fileStream = new FileStream(filePathName, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = XmlWriter.Create(fileStream, xmlWriterSettings))
			{
				var serializer = new XmlSerializer(typeof(MigrationPointVersionCollection));
				serializer.Serialize(writer, _migrationPointVersions);
				writer.Flush();
			}

			_filePathName = filePathName;
		}

		public bool Exists(string migrationPointName)
		{
			return _migrationPointVersions.Contains(migrationPointName);
		}

		public void Save()
		{
			if (_filePathName == null) throw new MigratorException("Save called without a set file path name to save to.");
			Save(_filePathName);
		}
	}
}

