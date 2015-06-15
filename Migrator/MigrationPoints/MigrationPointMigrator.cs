using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Fubineva.NOps.Migrator.MigrationPoints.Migrations;

namespace Fubineva.NOps.Migrator.MigrationPoints
{
	public class MigrationPointMigrator
	{
		private readonly Type _dpType;
		private readonly ProgressReport _progressMessage;
		private IMigrationPoint _migrationPoint;
		private IList<MigrationType> _requiredMigrationSequence;
		private bool _isReversableSequence;
		private string _backupLabel = null;
		private readonly object _migrationLock = new object();
		private Stack<IMigrateDown> _migrated;
		private static readonly string s_indent = new String(' ', 4);

		internal struct MigrationType
		{
			public Type Type;
			public long Number;
		}

		public MigrationPointMigrator(Type migrationPoint, ProgressReport progressMessage = null)
		{
			if (migrationPoint == null)
			{
				throw new ArgumentNullException("migrationPoint");
			}
			_dpType = migrationPoint;
			_progressMessage = progressMessage ?? ProgressReporting.NullMessageReceiver;
		}
		
		public virtual void Revert()
		{
			lock(_migrationLock)
			{
				if (!_isReversableSequence)
				{
					try
					{
						P("Restoring MigrationPoint.");
						MigrationPointInstance.Restore(_backupLabel);
						P("Restore completed.");
					}
					catch(Exception ex)
					{
						throw new MigrationReversalFailureException(string.Format("Migration reversal for MigrationPoint '{0}' failed. An attempted restore threw an exception.", MigrationPointName), ex);
					}
				}
				else
				{
					try
					{
						P("Reverting by downward migration.", MigrationPointName);
						foreach (var appliedMigration in _migrated)
						{
							P("Migrating down {0}", appliedMigration.GetType().Name);
							appliedMigration.Down();
							P("Downward migration completed.");
						}
						P("Migration reversal completed.");
					}
					catch(Exception ex)
					{
						throw new MigrationReversalFailureException(string.Format("Migration reversal for MigrationPoint '{0}' failed. Downward traversal threw an exeption.", MigrationPointName), ex);
					}   
				}
			}
		}

		[DebuggerStepThrough]
		private void P(string format, params object[] args)
		{
			P(string.Format(format, args));
		}

		[DebuggerStepThrough]
		private void P(string message)
		{
			_progressMessage(s_indent + message);
		}

		public virtual bool IsMigrationPointCurrent(long currentVersion, out long requiredVersion)
		{
			lock (_migrationLock)
			{
				requiredVersion = DetermineRequiredVersion();
				return requiredVersion <= currentVersion;
			}
		}

		private long DetermineRequiredVersion()
		{
			return GetMigrationPointMigrationTypes()
				.Select(GetMigrationAttribute)
				.Where(a => a != null) // migration types can miss their attribute (e.g. not ready for production yet)
				.Max(a => a.Number);
		}

		public virtual long Migrate(long currentVersion)
		{
			lock (_migrationLock)
			{
				_migrated = new Stack<IMigrateDown>();
				_requiredMigrationSequence = DetermineMigrationSequence(currentVersion).ToList();

				if (!_requiredMigrationSequence.Any())
				{
					P("Nothing to migrate, MigrationPoint is current.");
					return currentVersion;
				}
				
				P("Migrating MigrationPoint to {0}.", _requiredMigrationSequence.Last().Number);

				var migrationSequence = new Stack<MigrationType>(_requiredMigrationSequence.Reverse());

				_isReversableSequence = IsReversableSequence();

				_backupLabel = null;
				if(!_isReversableSequence)
				{
					_backupLabel = string.Format("pre-migration-{0}", currentVersion);
					P("Migration sequence does not support reverting through downward migration. Requesting backup {0}", _backupLabel);
					try
					{
						MigrationPointInstance.Backup(_backupLabel);
					}
					catch(Exception ex)
					{
						throw new BackupFailureException(
							string.Format("MigrationPoint '{0}' threw an expection while trying to create backup '{1}'",
										  MigrationPointName, _backupLabel), ex);
					}
				}
				else
				{
					P("Migration sequence supports downward migration. No backup requested.");
				}
				
				long newVersion = currentVersion;

				MigrationPointInstance.OnMigrationStarting();

				while(migrationSequence.Any())
				{
					var migrationType = migrationSequence.Pop();

					var instance = (IMigrate)Activator.CreateInstance(migrationType.Type);
					try
					{
						P("Running migration step '{0}' to get to version {1}.", migrationType.Type.Name, migrationType.Number);
						instance.Up();
						// only after a succesful Up we will push the migration on the down stack
						if(typeof(IMigrateDown).IsAssignableFrom(migrationType.Type))
						{
							_migrated.Push((IMigrateDown)instance);
						}
						newVersion = migrationType.Number;

						P("Migration step completed.");
					}
					catch(Exception ex)
					{
						MigrationPointInstance.OnMigrationFailed(migrationType.Number, ex);

						RevertMigrationSequence(migrationType.Number, ex);
					}
				}
				
				MigrationPointInstance.OnMigrationCompleted();

				P("MigrationPoint migrated to {0}.", newVersion);

				return newVersion;
			}
		}

		private void RevertMigrationSequence(long failingMigrationNumber, Exception ex)
		{
			if(_backupLabel != null)
			{
				P("Migration failed, reverting through a MigrationPoint restore."); 
				RevertMigrationThroughRestore(failingMigrationNumber, ex);
			}

			P("Migration failed, reverting by a downward migration."); 
			RevertMigrationByMigratingDown(failingMigrationNumber, ex);
		}

		private void RevertMigrationByMigratingDown(long failingMigrationNumber, Exception ex)
		{
			foreach(var reversable in _migrated)
			{
				try
				{
					P("Migrating down '{0}'.", reversable.GetType().Name);
					reversable.Down();
				}
				catch(Exception downEx)
				{
					throw new UnrevertedMigrationFailureException(string.Format("Migration {0} failed, migration reversal was attempted but also failed.", failingMigrationNumber), ex, downEx);
				}
				
			}
			throw new RevertedMigrationFailureException(
				string.Format("Migration {0} failed, migration reverted back down.", failingMigrationNumber), ex);
		}

		private void RevertMigrationThroughRestore(long failingMigrationNumber, Exception ex)
		{
			MigrationPointInstance.Restore(_backupLabel);
			throw new RevertedMigrationFailureException(string.Format("Migration {0} failed, restore was invoked.", failingMigrationNumber), ex);
		}

		private IMigrationPoint MigrationPointInstance
		{
			get
			{
				return _migrationPoint ?? (_migrationPoint = (IMigrationPoint)Activator.CreateInstance(_dpType));
			}
		}

		public string MigrationPointName
		{
			get
			{
				return _dpType.Name;
			}
		}

		private bool IsReversableSequence()
		{
			return _requiredMigrationSequence.All(mt => typeof(IMigrateDown).IsAssignableFrom(mt.Type));
		}

		internal virtual IEnumerable<MigrationType> DetermineMigrationSequence(long currentVersion)
		{
			return GetMigrationPointMigrationTypes()
				.Where(t => IsRequiredMigration(t, currentVersion))
				.Select(t => new MigrationType()
						{
							Number = GetMigrationAttribute(t).Number, 
							Type = t,
						}
				)
				.OrderBy(mt => mt.Number);
		}

		private static bool IsRequiredMigration(Type t, long currentVersion)
		{
			var attribute = GetMigrationAttribute(t);
			return attribute != null && IsUnappliedMigration(attribute, currentVersion);
		}

		private static MigrationAttribute GetMigrationAttribute(Type t)
		{
			return (MigrationAttribute)t.GetCustomAttributes(typeof(MigrationAttribute), false).SingleOrDefault();
		}

		private static bool IsUnappliedMigration(MigrationAttribute attribute, long currentVersion)
		{
			return attribute.Number > currentVersion;
		}

		// testing seam
		protected virtual IEnumerable<Type> GetMigrationPointMigrationTypes()
		{
			var dpNamespace = _dpType.Namespace;
			return _dpType.Assembly
				.GetTypes()
				.Where(
					t => t.Namespace == dpNamespace
						 && typeof(IMigrate).IsAssignableFrom(t)
						 && !t.IsAbstract
				);
		}
		
	}

	public class MigrationReversalFailureException : MigratorException
	{
		internal MigrationReversalFailureException(string message, Exception innerException) : base(message, innerException)
		{
			
		}
	}

	public class MigrationFailureException : MigratorException
	{
		internal MigrationFailureException(string message) : base(message)
		{
		}

		internal MigrationFailureException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}

	public class UnrevertedMigrationFailureException : MigrationFailureException
	{
		private readonly List<Exception> _reversalExceptions = new List<Exception>();

		internal UnrevertedMigrationFailureException(string message) : base(message)
		{
		}

		internal UnrevertedMigrationFailureException(string message, Exception innerException, List<Exception> reversalExceptions) 
			: base(message, innerException)
		{
			_reversalExceptions = reversalExceptions;
		}

		internal UnrevertedMigrationFailureException(string message, Exception innerException, Exception reversalException) 
			: this(message, innerException, new List<Exception>() { reversalException })
		{
			
		}

		internal UnrevertedMigrationFailureException(string message, Exception innerException) 
			: base(message, innerException)
		{
			
		}

		public List<Exception> ReversalExceptions
		{
			get
			{
				return _reversalExceptions;
			}
		}

		public override string ToString()
		{
			return base.ToString() + string.Join("\r\n", ReversalExceptions.ToString());
		}
	}

	public class RevertedMigrationFailureException : MigrationFailureException
	{
		internal RevertedMigrationFailureException(string message) : base(message)
		{
		}

		internal RevertedMigrationFailureException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	public class BackupFailureException : PreMigrationException
	{
		internal BackupFailureException(string message) : base(message)
		{
		}

		internal BackupFailureException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}

	public class PreMigrationException : MigratorException
	{
		internal PreMigrationException(string message) : base(message)
		{
		}

		internal PreMigrationException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}