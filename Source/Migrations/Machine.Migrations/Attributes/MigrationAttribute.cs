namespace Machine.Migrations
{
	using System;

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class MigrationAttribute : Attribute
	{
		private short version;
		private string scope;

		public MigrationAttribute(short version, string scope)
		{
			this.version = version;
			this.scope = scope;
		}

		public string Scope
		{
			get { return scope; }
			set { scope = value; }
		}

		public short Version
		{
			get { return version; }
			set { version = value; }
		}
	}
}
