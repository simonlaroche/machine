using System;
using System.Collections.Generic;

using Machine.BackgroundJobs.Sample;
using Machine.Core;

using NUnit.Framework;

using Rhino.Mocks;

namespace Machine.BackgroundJobs.Services.Impl
{
  [TestFixture]
  public class SimpleJobRepositoryTests : StandardFixture<SimpleJobRepository>
  {
    LongRunningJob _job1;
    LongRunningJob _job2;
    LongRunningJob _job3;

    public override SimpleJobRepository Create()
    {
      _job1 = new LongRunningJob();
      _job2 = new LongRunningJob();
      _job3 = new LongRunningJob();
      return new SimpleJobRepository();
    }

    [Test]
    public void FindCompletedJobs_Always_ReturnsOnlyThoseCompleted()
    {
      using (_mocks.Record())
      {
        _job2.IsComplete = true;
      }
      _target.AddJob(_job1);
      _target.AddJob(_job2);
      _target.AddJob(_job3);
      CollectionAssert.AreEqual(new IBackgroundJob[] {_job2}, new List<IBackgroundJob>(_target.FindCompletedJobs()));
      _mocks.VerifyAll();
    }

    [Test]
    public void FindActiveJobs_Always_ReturnsOnlyThoseNotCompleted()
    {
      using (_mocks.Record())
      {
        _job2.IsComplete = true;
      }
      _target.AddJob(_job1);
      _target.AddJob(_job2);
      _target.AddJob(_job3);
      CollectionAssert.AreEqual(new IBackgroundJob[] {_job1, _job3}, new List<IBackgroundJob>(_target.FindActiveJobs()));
      _mocks.VerifyAll();
    }

    [Test]
    public void FindJobs_Always_ReturnsAll()
    {
      using (_mocks.Record())
      {
        _job2.IsComplete = true;
      }
      _target.AddJob(_job1);
      _target.AddJob(_job2);
      _target.AddJob(_job3);
      CollectionAssert.AreEqual(new IBackgroundJob[] {_job1, _job2, _job3}, new List<IBackgroundJob>(_target.FindJobs()));
      _mocks.VerifyAll();
    }

    [Test]
    public void FindJob_IsThere_ReturnsThatJob()
    {
      using (_mocks.Record())
      {
      }
      LongRunningJob job;
      _target.AddJob(new LongRunningJob());
      _target.AddJob(job = new LongRunningJob());
      _target.AddJob(new LongRunningJob());
      Assert.AreEqual(job, _target.FindJob(2));
      _mocks.VerifyAll();
    }

    [Test]
    public void AddJob_NewJob_SetsJobNumber()
    {
      using (_mocks.Record())
      {
      }
      LongRunningJob job = new LongRunningJob();
      _target.AddJob(job);
      Assert.AreEqual(1, job.JobNumber);
      _mocks.VerifyAll();
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AddJob_OldJob_Throws()
    {
      using (_mocks.Record())
      {
      }
      LongRunningJob job;
      _target.AddJob(job = new LongRunningJob());
      _target.AddJob(job);
    }
  }
}