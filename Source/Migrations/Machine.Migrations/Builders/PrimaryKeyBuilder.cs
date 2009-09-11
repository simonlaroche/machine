namespace Machine.Migrations.Builders
{
  using System;
  using System.Collections.Generic;

  using SchemaProviders;

  public class PrimaryKeyBuilder : ColumnBuilder
  {
    public PrimaryKeyBuilder(string name, Type columnType) : base(name, columnType)
    {
    }

    public PrimaryKeyBuilder(string name, Type columnType, short? size) : base(name, columnType, size)
    {
    }

    public override Column Build(TableBuilder table, ISchemaProvider schemaBuilder, IList<PostProcess> posts)
    {
      Column col = base.Build(table, schemaBuilder, posts);
      col.IsPrimaryKey = true;
      col.AllowNull = false;
      return col;
    }
  }
}