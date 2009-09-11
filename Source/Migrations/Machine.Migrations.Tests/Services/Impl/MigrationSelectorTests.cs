using System;
using System.Collections.Generic;

using Machine.Core;
using Machine.Migrations.Core;

using NUnit.Framework;

using Rhino.Mocks;

namespace Machine.Migrations.Services.Impl
{
  [TestFixture]
  public class MigrationSelectorTests : StandardFixture<MigrationSelector>
  {
    IVersionStateFactory _versionStateFactory;
    IMigrationFinder _migrationFinder;
    List<MigrationReference> _migrations;

    [Test]
    public void SelectMigrations_DesiredVersionIs4_CurrentVersionIs0_LastMigrationIs4_IsAll()
    {
      using (_mocks.Record())
      {
        SetupMocks(4, 0);
      }
      List<MigrationStep> actual = new List<MigrationStep>(_target.SelectMigrations());
      CollectionAssert.AreEqual(
        new MigrationStep[]
        {
          new MigrationStep(_migrations[0], false),
          new MigrationStep(_migrations[1], false),
          new MigrationStep(_migrations[2], false),
          new MigrationStep(_migrations[3], false),
        },
        actual
        );
    }

    [Test]
    public void SelectMigrations_DesiredVersionIs4_CurrentVersionIs0ButAsRougueApplied_LastMigrationIs4_IsAllButApplied()
    {
      using (_mocks.Record())
      {
        SetupMocks(4, new short[] {2});
      }
      List<MigrationStep> actual = new List<MigrationStep>(_target.SelectMigrations());
      CollectionAssert.AreEqual(
        new MigrationStep[]
        {
          new MigrationStep(_migrations[0], false),
          new MigrationStep(_migrations[2], false),
          new MigrationStep(_migrations[3], false),
        },
        actual
        );
    }

    [Test]
    public void SelectMigrations_DesiredVersionIs4_CurrentVersionIs4_LastMigrationIs4_IsNothing()
    {
      using (_mocks.Record())
      {
        SetupMocks(4, 4);
      }
      List<MigrationStep> actual = new List<MigrationStep>(_target.SelectMigrations());
      CollectionAssert.IsEmpty(actual);
    }

    [Test]
    public void SelectMigrations_DesiredVersionIs4_CurrentVersionIs2_LastMigrationIs4_IsLastTwo()
    {
      using (_mocks.Record())
      {
        SetupMocks(4, 2);
      }
      List<MigrationStep> actual = new List<MigrationStep>(_target.SelectMigrations());
      CollectionAssert.AreEqual(
        new MigrationStep[]
        {
          new MigrationStep(_migrations[2], false),
          new MigrationStep(_migrations[3], false),
        },
        actual
        );
    }

    [Test]
    public void SelectMigrations_DesiredVersionIs1_CurrentVersionIs2_LastMigrationIs4_IsSecondMigrationReverted()
    {
      using (_mocks.Record())
      {
        SetupMocks(1, 2);
      }
      List<MigrationStep> actual = new List<MigrationStep>(_target.SelectMigrations());
      CollectionAssert.AreEqual(
        new MigrationStep[]
        {
          new MigrationStep(_migrations[1], true),
        },
        actual
        );
    }

    [Test]
    public void SelectMigrations_DesiredVersionIs0_CurrentVersionIs4_LastMigrationIs4_IsAllMigrationsReverted()
    {
      using (_mocks.Record())
      {
        SetupMocks(0, 4);
      }
      List<MigrationStep> actual = new List<MigrationStep>(_target.SelectMigrations());
      CollectionAssert.AreEqual(
        new MigrationStep[]
        {
          new MigrationStep(_migrations[3], true),
          new MigrationStep(_migrations[2], true),
          new MigrationStep(_migrations[1], true),
          new MigrationStep(_migrations[0], true),
        },
        actual
        );
    }

    [Test]
    public void SelectMigrations_DesiredVersionIs0_CurrentVersionIs2_LastMigrationIs4_IsFirstTwoMigrationsReverted()
    {
      using (_mocks.Record())
      {
        SetupMocks(0, 2);
      }
      List<MigrationStep> actual = new List<MigrationStep>(_target.SelectMigrations());
      CollectionAssert.AreEqual(
        new MigrationStep[]
        {
          new MigrationStep(_migrations[1], true),
          new MigrationStep(_migrations[0], true),
        },
        actual
        );
    }

    [Test]
    public void SelectMigrations_NoMigrations_IsEmpty()
    {
      using (_mocks.Record())
      {
        _migrations.Clear();
        SetupMocks(0, 0);
      }
      List<MigrationStep> actual = new List<MigrationStep>(_target.SelectMigrations());
      CollectionAssert.IsEmpty(actual);
    }

    void SetupMocks(short desired, short current)
    {
      List<short> applied = new List<short>();
      for (short i = 1; i <= current; ++i)
      {
        applied.Add(i);
      }
      SetupMocks(desired, applied.ToArray());
    }

    void SetupMocks(short desired, IList<short> applied)
    {
      SetupResult.For(_versionStateFactory.CreateVersionState(_migrations)).Return(new VersionState(4, desired, applied));
      SetupResult.For(_migrationFinder.FindMigrations()).Return(_migrations);
    }

    public override MigrationSelector Create()
    {
      _migrationFinder = _mocks.DynamicMock<IMigrationFinder>();
      _versionStateFactory = _mocks.DynamicMock<IVersionStateFactory>();
      _migrations = new List<MigrationReference>();
      _migrations.Add(new MigrationReference(1, "A", "001_a.cs"));
      _migrations.Add(new MigrationReference(2, "B", "002_b.cs"));
      _migrations.Add(new MigrationReference(3, "C", "003_c.cs"));
      _migrations.Add(new MigrationReference(4, "D", "004_d.cs"));
      return new MigrationSelector(_migrationFinder, _versionStateFactory);
    }
  }
}