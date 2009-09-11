using System;
using System.Threading;

using Machine.Utility.ThreadPool.QueueStrategies;

using NUnit.Framework;

namespace Machine.Utility.ThreadPool
{
  [TestFixture]
  public class QueuedWithABatchAndLetDormant : ThreadPoolFixture
  {
    [Test]
    public void Queued_And_Then_Left_Over_A_Period_Of_Time()
    {
      MessageConsumer consumer = new MessageConsumer();
      ThreadPool pool = new ThreadPool(ThreadPoolConfiguration.FiveAndTen, new SingleQueueStrategy());
      pool.Start();
      foreach (Message message in MessageBuilder.MakeMessages(40))
      {
        pool.Queue(consumer, message);
      }
      Thread.Sleep(TimeSpan.FromSeconds(10.0));
      pool.Stop();
      Assert.AreEqual(40, consumer.NumberOfMessagesProcessed);
    }

    [Test]
    public void Queued_And_Then_Left_Over_A_Period_Of_Time_With_Queue_Per_Worker()
    {
      MessageConsumer consumer = new MessageConsumer();
      ThreadPool pool = new ThreadPool(ThreadPoolConfiguration.FiveAndTen, new QueuePerWorkerStrategy());
      pool.Start();
      foreach (Message message in MessageBuilder.MakeMessages(30))
      {
        pool.Queue(consumer, message);
      }
      Thread.Sleep(TimeSpan.FromSeconds(10.0));
      pool.Stop();
      Assert.AreEqual(30, consumer.NumberOfMessagesProcessed);
    }

    [Test]
    public void Queued_And_Then_Left_Over_A_Period_Of_Time_With_Affinity_Strategy()
    {
      MessageConsumer consumer = new MessageConsumer();
      ThreadPool pool = new ThreadPool(ThreadPoolConfiguration.FiveAndTen, new QueueAffinityStrategy<Message, string>());
      pool.Start();
      foreach (Message message in MessageBuilder.MakeMessages(30))
      {
        pool.Queue(consumer, message);
      }
      Thread.Sleep(TimeSpan.FromSeconds(10.0));
      pool.Stop();
      Assert.AreEqual(30, consumer.NumberOfMessagesProcessed);
    }
  }
}