using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Fubineva.NOps.Migrator.Tests.Helpers
{
	public class TempFileLocation : IDisposable
	{
		private readonly string _path;

		private TempFileLocation(string rootPath)
		{
			var path = CreateTemporaryPath(rootPath);

			_path = path;
		}

		public string Path
		{
			get { return _path; }
		}

		private static string CreateTemporaryPath(string rootPath)
		{
			var random = new Random();
			string path;

			int max = int.Parse("FFFF", NumberStyles.HexNumber);
			do
			{
				path = global::System.IO.Path.Combine(rootPath, random.Next(max).ToString("X4"));
			}
			while (Directory.Exists(path));

			Directory.CreateDirectory(path);
			return path;
		}

		public void Dispose()
		{
			if (Directory.Exists(Path))
			{
				Directory.Delete(Path, true);
			}
		}

		public static TempFileLocation Create()
		{
			var rootPath = global::System.IO.Path.Combine(AppPath(), "TestFiles");
			if (!Directory.Exists(rootPath))
			{
				Directory.CreateDirectory(rootPath);
			}

			return new TempFileLocation(rootPath);
		}

		private static string AppPath()
		{
			var appPath = Assembly.GetExecutingAssembly().Location;
			return global::System.IO.Path.GetDirectoryName(appPath);
		}

		public string File(string relativeFilePathName)
		{
			return global::System.IO.Path.Combine(Path, relativeFilePathName);
		}
	}
}