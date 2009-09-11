namespace Machine.Migrations.Services.Impl
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	public class AssemblyMigrationFinder : IMigrationFinder
	{
		private readonly Assembly assembly;
		private readonly IConfiguration configuration;

		public AssemblyMigrationFinder(Assembly assembly, IConfiguration configuration)
		{
			this.assembly = assembly;
			this.configuration = configuration;
		}

		public ICollection<MigrationReference> FindMigrations()
		{
			var refs = new List<MigrationReference>();

			foreach (var type in assembly.GetTypes())
			{
				if (type.IsPublic && type.IsClass && !type.IsAbstract && typeof (IDatabaseMigration).IsAssignableFrom(type))
				{
					var attrs = type.GetCustomAttributes(typeof (MigrationAttribute), false);

					if (attrs.Length == 0)
					{
						throw new Exception("Found migration type that has no version information. See type " + type.FullName);
					}

					var att = (MigrationAttribute) attrs[0];

					if (att.Scope == configuration.Scope)
					{
						var mref = new MigrationReference(att.Version, type.Name, "");
						mref.Reference = type;

						refs.Add(mref);
					}
				}
			}

			refs.Sort(delegate(MigrationReference mr1, MigrationReference mr2) { return mr1.Version.CompareTo(mr2.Version); });

			return refs;
		}
	}
}