﻿using System;
using System.Collections.Generic;

using Machine.Migrations.Core;

namespace Machine.Migrations.Services.Impl
{
  public class VersionStateFactory : IVersionStateFactory
  {
    #region Member Data
    readonly IConfiguration _configuration;
    readonly ISchemaStateManager _schemaStateManager;
    #endregion

    #region VersionStateFactory()
    public VersionStateFactory(IConfiguration configuration, ISchemaStateManager schemaStateManager)
    {
      _configuration = configuration;
      _schemaStateManager = schemaStateManager;
    }
    #endregion

    #region IVersionStateFactory Members
    public VersionState CreateVersionState(ICollection<MigrationReference> migrations)
    {
      short[] applied = _schemaStateManager.GetAppliedMigrationVersions(_configuration.Scope);
      short desired = _configuration.DesiredVersion;
      short last = 0;
      if (migrations.Count > 0)
      {
        List<MigrationReference> all = new List<MigrationReference>(migrations);
        last = all[all.Count - 1].Version;
      }
      if (desired > last)
      {
        throw new ArgumentException("DesiredVersion is greater than maximum migration!");
      }
      if (desired < 0)
      {
        desired = last;
      }
      return new VersionState(last, desired, applied);
    }
    #endregion
  }
}