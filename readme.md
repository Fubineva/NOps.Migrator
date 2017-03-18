# NOps.Migrator #

## Introduction ##
NOps.Migrator is a small code based migration framework. It allows migrating data and configuration structures using C# code. It's comparable to various available migration frameworks without being tied to SQL. It has no DSL of any kind, migrations are expressed in C#.

The framework was created to bring a simular migration methaphor when needing to change data on the file system, transform XML file to a new schema and mutate and clean up configuration files. 

NOps.Migrator tracks what version a "migration point" has and figures out which migrations have yet to run. It handles migration failures by reverting down or invoking a backup restore operation.

## Status ##
NOps.Migrator is in *alpha* phase. The code is fairly feature complete, decently tested and has had some use in production.

## Concepts ##
The framework works with *MigrationPoints*. Each *MigrationPoint* has a stack of migrations. Each migration is ordered by a sequencial number. Migrations may support a downward operation, sort of an undo, the inverse of the upward migration.

Failing migrations are reverted either through downward operations or through invoking a **MigrationPoint**'s restore operation. To support the latter a *MigrationPoint* implementation is expect to provide a backup and restore operation over the data or configuration it manages.

Migrations are defined by the user by creating classes that implement the **IMigrate** interface and include a class level *Migrate* attribute that defines the migration's (sequencial) number. The migration classes are placed in the same namespace as the *MigrationPoint* it belongs to.

Example:
	
	[Migration(1)]
	public class Migration1 : IMigrate
	{
		public void Up()
	    {
	    	// mutate stuff here... 
	    }
	}


If the class is able to reverse it's "up" migration an additional **IMigrateDown** interface can be implemented.

MigrationPoints are defined by creating a class implementing the **IMigrationPoint** interface. As stated earlier, the interface asks an implementation to provide a **Backup** and **Restore** operation.

Migrations are done using either the **Migrator**, which acts on unit of *MigrationPoint*s. If one *MigrationPoint* fails to migrate, all MigrationPoints in the unit are reverted.

Additionally the **Migrator** works with a **IMigrationPointVersionRegistry** to figure out the current version of *MigrationPoint*s. The framework offers one implementation, the **MigrationPointVersionRegistry** class that stores version information in an XML file.

Migrations can also be done per *MigrationPoint* using the **MigrationPointMigrator**. The user must supply the *MigrationPoint*'s current version, it's not aware of **IMigrationPointVersionRegistry**. It can however be used indirectly by the user.

Failure conditions and resulting migration reversals are expressed in exceptions. The framework throws **UnrevertedMigrationFailureException** which is the most critical: data has ended in a corrupt state. **RevertedMigrationFailureException** indicates a failing migration but a succesful reversal. Examine the exception's inner exception and ReversalExceptions set to find out more about migration failures.

Using the **Migrator** a failing *MigrationPoint* migration results in reversal of the failing *MigrationPoint* and all the *MigrationPoint*s it migrated succesfully during it's invocation. It handles them as a unit of work.

The framework provides basic tracing by providing the **Migrator** or **MigrationPointMigrator** a **ProgressReport** delegate.

For code examples review the unit-test project.

## Changed alpha 0.4 ##
- ported to .NET core

## Changes alpha 0.3 ##
- Renamed DataPoint to MigrationPoint (it's not only about data but also about configuration).
- It's now possible to check if a migration is required without invoking the actual migration sequence.
- Fix: MigrationPoints without any migrations results in exception.
- Can now skip migrations by removing the Migrate attribute.
- Progress reporting improvements: Now being more verbose about versioning requirements and targets.

License: MIT


