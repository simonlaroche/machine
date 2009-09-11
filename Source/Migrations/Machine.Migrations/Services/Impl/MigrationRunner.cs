using System;
using System.Collections.Generic;
using System.Data;

namespace Machine.Migrations.Services.Impl
{
  public class MigrationRunner : IMigrationRunner
  {
    #region Logging
    static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(MigrationRunner));
    #endregion

    #region Member Data
    readonly IMigrationFactoryChooser _migrationFactoryChooser;
    readonly IMigrationInitializer _migrationInitializer;
    readonly ISchemaStateManager _schemaStateManager;
    readonly IConfiguration _configuration;
    readonly ITransactionProvider _transactionProvider;
    #endregion

    #region MigrationRunner()
    public MigrationRunner(IMigrationFactoryChooser migrationFactoryChooser, IMigrationInitializer migrationInitializer,
      ISchemaStateManager schemaStateManager, IConfiguration configuration, ITransactionProvider transactionProvider)
    {
      _schemaStateManager = schemaStateManager;
      _transactionProvider = transactionProvider;
      _configuration = configuration;
      _migrationInitializer = migrationInitializer;
      _migrationFactoryChooser = migrationFactoryChooser;
    }
    #endregion

    #region IMigrationRunner Members
    public bool CanMigrate(ICollection<MigrationStep> steps)
    {
      foreach (MigrationStep step in steps)
      {
        MigrationReference migrationReference = step.MigrationReference;
        IMigrationFactory migrationFactory = _migrationFactoryChooser.ChooseFactory(migrationReference);
        IDatabaseMigration migration = migrationFactory.CreateMigration(migrationReference);
        step.DatabaseMigration = migration;
        _migrationInitializer.InitializeMigration(migration);
      }
      _log.Info("All migrations are initialized.");
      return true;
    }

    public void Migrate(ICollection<MigrationStep> steps)
    {
      foreach (MigrationStep step in steps)
      {
        using (Machine.Core.LoggingUtilities.Log4NetNdc.Push("{0}", step.MigrationReference.Name))
        {
          _log.InfoFormat("Running {0}", step);
          if (!_configuration.ShowDiagnostics)
          {
            IDbTransaction transaction = null;
            try
            {
              transaction = _transactionProvider.Begin();
              step.Apply();
              if (step.Reverting)
              {
                _schemaStateManager.SetMigrationVersionUnapplied(step.Version, _configuration.Scope);
              }
              else
              {
                _schemaStateManager.SetMigrationVersionApplied(step.Version, _configuration.Scope);
              }
              _log.InfoFormat("Comitting");
              transaction.Commit();
            }
            catch (Exception)
            {
              if (transaction != null)
              {
                _log.InfoFormat("Rollback");
                transaction.Rollback();
              }
              throw;
            }
          }
        }
      }
    }
    #endregion
  }
}