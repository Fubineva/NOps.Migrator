using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using NOps.Migrator.MigrationPoints;
using NOps.Migrator.Registry;

namespace NOps.Migrator
{
	public class Migrator
	{
		private readonly IEnumerable<Type> _migrationPointTypes;
		private readonly IMigrationPointVersionRegistry _registry;
		private Stack<MigrationPointMigrator> _migrated;

		private readonly object _migratorLock = new object();
		private readonly ProgressReport _progressMessage;

		public Migrator(IMigrationPointVersionRegistry registry, ProgressReport progressReport = null)
			: this(DetermineAssemblyMigrationPoints(Assembly.GetCallingAssembly()), registry, progressReport)
		{
		}

		public Migrator(Assembly migrationPointAssembly, IMigrationPointVersionRegistry registry, ProgressReport progressReport = null)
			: this(DetermineAssemblyMigrationPoints(migrationPointAssembly), registry, progressReport)
		{
		}

		public Migrator(IEnumerable<Type> migrationPointTypes, IMigrationPointVersionRegistry registry, ProgressReport progressReport = null)
		{
			_migrationPointTypes = migrationPointTypes;
			_registry = registry;
			_progressMessage = progressReport ?? ProgressReporting.NullMessageReceiver;
		}

		[DebuggerStepThrough]
		private void P(string format, params object[] args)
		{
			P(string.Format(format, args));
		}

		[DebuggerStepThrough]
		private void P(string message)
		{
			_progressMessage(message);
		}

		internal static IEnumerable<Type> DetermineAssemblyMigrationPoints(Assembly migrationPointAssembly)
		{
			return migrationPointAssembly
				.GetTypes()
				.Where(t => typeof(IMigrationPoint).IsAssignableFrom(t) && !t.IsAbstract);
		}

		public virtual void Migrate()
		{
			lock (_migratorLock)
			{
				_migrated = new Stack<MigrationPointMigrator>();

				P("Migration of MigrationPoints started.");
				foreach(var migrationPoint in _migrationPointTypes)
				{
					MigrateMigrationPoint(migrationPoint);
				}
				P("Migration of MigrationPoints completed.");
			}
		}

		public virtual bool IsDataCurrent()
		{
			bool any = false;
			lock (_migratorLock)
			{
				P("Checking for required migrations....");
				foreach(var migrationPointType in _migrationPointTypes)
				{
					long version = 0;

					if(_registry.Exists(migrationPointType.Name))
					{
						MigrationPointVersion registryEntry = _registry[migrationPointType.Name];
						version = registryEntry.Version;
					}

					var dtMigrator = CreateMigrationPointMigrator(migrationPointType);

					long requiredVersion;
					if (!dtMigrator.IsMigrationPointCurrent(version, out requiredVersion))
					{
						P("MigrationPoint '{0}' requires migration from {1} to {2}.", migrationPointType.Name, version, requiredVersion);
						any = true;
					}
					P("MigrationPoint '{0}' is current (version {1}).", migrationPointType.Name, version);
				}
			}

			if (!any)
			{
				P("All MigrationPoints are current, no migration required.");	
			}
			else
			{
				P("MigrationPoints found requiring migrations.");
			}
			
			return !any;
		}

		private void MigrateMigrationPoint(Type migrationPoint)
		{
			long version = 0;

			MigrationPointVersion registryEntry = null;
			if(_registry.Exists(migrationPoint.Name))
			{
				registryEntry = _registry[migrationPoint.Name];
				version = registryEntry.Version;
			}

			var dtMigrator = CreateMigrationPointMigrator(migrationPoint);

			try
			{
				P("Migrating MigrationPoint {0} from version {1}.", migrationPoint.Name, version);
				long newVersion = dtMigrator.Migrate(version);
				if(newVersion > version)
				{
					_migrated.Push(dtMigrator);

					if(registryEntry == null)
					{
						_registry.Add(migrationPoint.Name, newVersion);
						P("Added '{0}' to the MigrationPointVersionRegistry.", migrationPoint.Name);
					}
					else
					{
						registryEntry.Version = newVersion;
					}

					_registry.Save();
				}
			}
			catch(MigrationFailureException ex)
			{
				RevertPreviouslyCompletedMigrationPointMigrations(migrationPoint.Name, version, ex);
				// should not get here (above method always throws)
				throw;
			}
		}

		private void RevertPreviouslyCompletedMigrationPointMigrations(string migrationPointName, long attemptedVersion, Exception ex)
		{
			P("MigrationPoint migration failed, reverting.");

			var revertFailures = new List<Exception>();
			while(_migrated.Any())
			{
				var dtm = _migrated.Pop();
				try
				{
					P("Reverting MigrationPoint {0}", dtm.MigrationPointName);
					dtm.Revert();
				}
				catch(Exception revertEx)
				{
					P("Revert failed.");
					revertFailures.Add(revertEx);
				}
			}
			
			P("All MigrationPoints reverted.");

			if (revertFailures.Any())
			{
				if (ex is UnrevertedMigrationFailureException)
				{
					throw new UnrevertedMigrationFailureException(string.Format("MigrationPoint '{0}' failed to migrate to version {1} and failed reverting. Reverted all other migrations, some of them also failed. Examine this ReversalExceptions on this exception.", migrationPointName, attemptedVersion), ex, revertFailures);    
				}
				
				throw new UnrevertedMigrationFailureException(string.Format("MigrationPoint '{0}' failed to migrate to version {1} and succesfully reverted. Reverted all other migrations, some of them failed. Examine this ReversalExceptions on this exception.", migrationPointName, attemptedVersion), ex, revertFailures);
			}

			if (ex is UnrevertedMigrationFailureException)
			{
				throw new UnrevertedMigrationFailureException(string.Format("MigrationPoint '{0}' failed to migrate to version {1} and failed reverting. Reverted all other migrations succesfully.", migrationPointName, attemptedVersion), ex);
			}

			throw new RevertedMigrationFailureException(string.Format("MigrationPoint '{0}' failed to migrate to version {1}. Reverted all migrations succesfully.", migrationPointName, attemptedVersion), ex);
		}

		protected virtual MigrationPointMigrator CreateMigrationPointMigrator(Type migrationPoint)
		{
			return new MigrationPointMigrator(migrationPoint, _progressMessage);
		}

	}

	public class MigratorException : Exception
	{
		public MigratorException(string message)
			: base(message)
		{
		}

		public MigratorException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}