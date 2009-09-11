using System;
using System.Collections.Generic;

using Machine.Migrations.SchemaProviders;

namespace Machine.Migrations.Builders
{
  public class ForeignKeyBuilder : ColumnBuilder
  {
    readonly string targetTable;
    readonly string targetColName;

    public ForeignKeyBuilder(string name, TableInfo referencedTable) : base(name)
    {
      targetTable = referencedTable.Name;
      targetColName = referencedTable.PrimaryKeyName;

      base.colType = referencedTable.PrimaryKeyType;
      base.size = referencedTable.PrimaryKeySize;
    }

    public ForeignKeyBuilder(string name, Type type, string targetTable, string targetColName) : base(name)
    {
      this.targetTable = targetTable;
      this.targetColName = targetColName;
      base.type = type;
    }

    public override Column Build(TableBuilder table, ISchemaProvider schemaProvider, IList<PostProcess> posts)
    {
      Column col = base.Build(table, schemaProvider, posts);

      posts.Add(new PostProcess(
        delegate()
        {
          string fkName = "FK_" +
            SchemaUtils.Normalize(table.Name) + "_" +
              SchemaUtils.Normalize(col.Name) + "_" +
                SchemaUtils.Normalize(targetColName);

          schemaProvider.AddForeignKeyConstraint(
            table.Name, fkName, col.Name,
            targetTable, targetColName);
        }));

      return col;
    }
  }
}