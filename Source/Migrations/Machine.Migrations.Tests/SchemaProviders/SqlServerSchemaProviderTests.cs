using System;

using Machine.Core;
using Machine.Migrations.DatabaseProviders;

using NUnit.Framework;

using Rhino.Mocks;

namespace Machine.Migrations.SchemaProviders
{
  [TestFixture]
  public class SqlServerSchemaProviderTests : StandardFixture<SqlServerSchemaProvider>
  {
    IDatabaseProvider _databaseProvider;

    public override SqlServerSchemaProvider Create()
    {
      _databaseProvider = _mocks.StrictMock<IDatabaseProvider>();
      return new SqlServerSchemaProvider(_databaseProvider);
    }

    [Test]
    public void DropTable_Always_Drops()
    {
      using (_mocks.Record())
      {
        SetupResult.For(_databaseProvider.ExecuteNonQuery("DROP TABLE {0}", "TheTable")).Return(true);
      }
      _target.DropTable("TheTable");
    }

    [Test]
    public void DropColumn_NoTable_DoesntDoDrop()
    {
      using (_mocks.Record())
      {
        SetupResult.For(_databaseProvider.ExecuteNonQuery("ALTER TABLE {0} DROP COLUMN \"{1}\"", "TheTable", "TheColumn"))
          .Return(true);
      }
      _target.RemoveColumn("TheTable", "TheColumn");
    }

    [Test]
    public void HasColumn_HasTableAndColumn_IsTrue()
    {
      using (_mocks.Record())
      {
        SetupResult.For(
          _databaseProvider.ExecuteScalar<Int32>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'",
            "TheTable", "TheColumn")).Return(1);
      }
      Assert.IsTrue(_target.HasColumn("TheTable", "TheColumn"));
    }

    [Test]
    public void HasColumn_HasTableAndNoColumn_IsFalse()
    {
      using (_mocks.Record())
      {
        SetupResult.For(
          _databaseProvider.ExecuteScalar<Int32>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'",
            "TheTable", "TheColumn")).Return(0);
      }
      Assert.IsFalse(_target.HasColumn("TheTable", "TheColumn"));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddTable_NoColumns_Throws()
    {
      using (_mocks.Record())
      {
      }
      _target.AddTable("TheTable", new Column[0]);
    }

    [Test]
    public void AddTable_OneColumnPrimaryKey_IsSql()
    {
      using (_mocks.Record())
      {
        SetupResult.For(
          _databaseProvider.ExecuteNonQuery(
            "CREATE TABLE TheTable (\r\n\"Id\" INT NOT NULL,\r\nCONSTRAINT PK_THETABLE_ID PRIMARY KEY CLUSTERED (\"ID\")\r\n)"))
          .Return(true);
      }
      Column[] columns = new Column[]
      {
        new Column("Id", typeof(Int32), 4, true) {IsIdentity = false},
      };
      _target.AddTable("TheTable", columns);
    }

    [Test]
    public void AddTable_OneColumnPrimaryKeyIdentity_IsSql()
    {
      using (_mocks.Record())
      {
        SetupResult.For(
          _databaseProvider.ExecuteNonQuery(
            "CREATE TABLE TheTable (\r\n\"Id\" INT NOT NULL IDENTITY(1, 1),\r\nCONSTRAINT PK_THETABLE_ID PRIMARY KEY CLUSTERED (\"ID\")\r\n)"))
          .Return(true);
      }
      Column[] columns = new Column[]
      {
        new Column("Id", typeof(Int32), 4, true),
      };
      _target.AddTable("TheTable", columns);
    }

    [Test]
    public void AddTable_TwoColumnsPrimaryKeyAndAStringWithMaxLength_IsSql()
    {
      using (_mocks.Record())
      {
        SetupResult.For(_databaseProvider.ExecuteNonQuery("CREATE TABLE TheTable " +
          "(\r\n\"Id\" INT NOT NULL,\r\n\"Name\" NVARCHAR(150) NOT NULL,\r\n" +
            "CONSTRAINT PK_THETABLE_ID PRIMARY KEY CLUSTERED (\"ID\")\r\n)")).Return(true);
      }
      Column[] columns = new Column[]
      {
        new Column("Id", typeof(Int32), 4, true) {IsIdentity = false},
        new Column("Name", typeof(String), 0),
      };
      _target.AddTable("TheTable", columns);
    }

    [Test]
    public void AddTable_TwoColumnsPrimaryKeyAndAStringWithLength_IsSql()
    {
      using (_mocks.Record())
      {
        SetupResult.For(_databaseProvider.ExecuteNonQuery("CREATE TABLE TheTable " +
          "(\r\n\"Id\" INT NOT NULL,\r\n\"Name\" NVARCHAR(100) NOT NULL,\r\n" +
            "CONSTRAINT PK_THETABLE_ID PRIMARY KEY CLUSTERED (\"ID\")\r\n)")).Return(true);
      }
      Column[] columns = new Column[]
      {
        new Column("Id", typeof(Int32), 4, true) {IsIdentity = false},
        new Column("Name", typeof(String), 100),
      };
      _target.AddTable("TheTable", columns);
    }

    [Test]
    public void RenameColumn_Always_DoesExec()
    {
      using (_mocks.Record())
      {
        SetupResult.For(_databaseProvider.ExecuteNonQuery("EXEC sp_rename '{0}.{1}', '{2}', 'COLUMN'", "TheTable",
          "OldColumn", "NewColumn")).Return(true);
      }
      _target.RenameColumn("TheTable", "OldColumn", "NewColumn");
    }

    [Test]
    public void RenameTable_Always_DoesExec()
    {
      using (_mocks.Record())
      {
        SetupResult.For(_databaseProvider.ExecuteNonQuery("EXEC sp_rename '{0}', '{1}'", "TheTable", "NewTable")).Return
          (true);
      }
      _target.RenameTable("TheTable", "NewTable");
    }

    [Test]
    public void AddUniqueConstraint_Always_HandlesSingleColumn()
    {
      using (_mocks.Record())
      {
        SetupResult.For(_databaseProvider.ExecuteNonQuery(
          "ALTER TABLE {0} ADD CONSTRAINT \"{1}\" UNIQUE NONCLUSTERED ({2})",
          "TheTable", "UniqueKeyName", "\"Name\" ASC")).Return(true);
      }
      _target.AddUniqueConstraint("TheTable", "UniqueKeyName", "Name");
    }

    [Test]
    public void AddUniqueConstraint_Always_HandlesMultipleColumns()
    {
      using (_mocks.Record())
      {
        SetupResult.For(_databaseProvider.ExecuteNonQuery(
          "ALTER TABLE {0} ADD CONSTRAINT \"{1}\" UNIQUE NONCLUSTERED ({2})",
          "TheTable", "UniqueKeyName", "\"Name\" ASC, \"Email\" ASC, \"Key\" ASC")).Return(true);
      }
      _target.AddUniqueConstraint("TheTable", "UniqueKeyName", "Name", "Email", "Key");
    }
  }
}