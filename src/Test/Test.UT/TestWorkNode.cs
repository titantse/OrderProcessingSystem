
using System.Collections.Generic;

namespace OrderProcessing.Test.UT
{
    using System.Collections.Concurrent;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OrderProcessing.Domain;
    using System.Threading;
    using OrderProcessing.Configuration;
    using OrderProcessing.Performance;
    using OrderProcessing.WorkNode;
    using OrderProcessing.DataAccessor;

    [TestClass]
    public class TestWorkNode
    {

        private void ResetPerfCount()
        {
            GeneralPerfCounter.Instance.Stat.Reset();
            PerfCounters.WorkerPerf = GeneralPerfCounter.Instance;
            PerfCounters.SchedulerPerf = GeneralPerfCounter.Instance;
        }

        private WorkNodeConfiguration TestConfiguration(int concurrentWokringThreads = 5, int pullingCountEachTime = 30, int maxQueueSize= 100)
        {
            WorkNodeConfiguration configuration = new WorkNodeConfiguration()
            {
                WorkNode = new WorkNodeElement() { MaxConcurrentWorkingThreads = concurrentWokringThreads },
                Monitor = new MonitorConfiguration()
                {
                    HeartBeatIntervalSeconds = 5,
                    MaxNoHeartBeatIntervalSeconds = 50,
                },
                Scheduler = new ScheduleConfiguration()
                {
                    PullingTasksIntervalSeconds = 1,
                    PullingTasksEachTime = pullingCountEachTime,
                    MaxQueueLength = maxQueueSize,
                    ProcessingTimedOutSeconds = 3
                },
                MaxWaitSecondsWhenStopping = 3
            };
            return configuration;
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestOrderWorkers()
        {
            int testSize = 10000;
            BlockingCollection<OrderProcessingInfo> queue = TestDataManager.NewOrderProcessinInfoQueue(testSize);
            WorkNodeElement ele = new WorkNodeElement()
            {
                MaxConcurrentWorkingThreads = 5,
            };
            TestOrderProcessor processor = new TestOrderProcessor();
            OrderWorkers workers = new OrderWorkers("BVT", ele, queue, processor);
            ResetPerfCount();
            workers.Start();
            while (queue.Count > 0)
            {
                Thread.Sleep(2000);
            }
            workers.Stop();
            Assert.AreEqual(testSize, processor.PreProcessed);
            Assert.AreEqual(testSize, processor.Processed);
            Assert.AreEqual(testSize, processor.PostProcessed);

            Assert.AreEqual(testSize, Statistic.Stat.ProcessCompleted);
            Assert.AreEqual(testSize, Statistic.Stat.Processed);

            workers.Start();
            workers.Start();
            workers.Stop();

        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestOrderScheduler()
        {
            int testSize = 200;
            int totalSize = testSize * 6;
            TestRepository reprository = new TestRepository(
                TestDataManager.NewOrderProcessinInfoQueue(testSize * 3),
                TestDataManager.NewOrderProcessinInfoQueue(testSize * 2),
                TestDataManager.NewOrderProcessinInfoQueue(testSize));
            DataAccessor.OrderRepository = reprository;
            DataAccessor.NodeMonitor = reprository;

            var processingQueue = new BlockingCollection<OrderProcessingInfo>();

            TestOrderProcessor processor = new TestOrderProcessor();
            NodeScheduler scheduler = new NodeScheduler("BVT",
                TestConfiguration(), processingQueue, processor);

            ResetPerfCount();

            scheduler.Start();
            while (!reprository.IsEmpty)
            {
                Thread.Sleep(2000);
            }
            scheduler.Stop();

            Assert.AreEqual(totalSize, processor.PreProcessed);
            Assert.AreEqual(totalSize, processor.Processed);
            Assert.AreEqual(totalSize,processor.PostProcessed);

            Assert.AreEqual(totalSize, Statistic.Stat.ProcessCompleted);
            Assert.AreEqual(totalSize, Statistic.Stat.Processed);

            scheduler.Start();
            scheduler.Start();
            scheduler.Stop();
            scheduler.Stop();
        }


        [TestMethod]
        [TestCategory("BVT")]
        public void TestMultipleSchedulers()
        {
            int testSize = 500;
            int totalSize = testSize * 6;
            TestRepository reprository = new TestRepository(
                TestDataManager.NewOrderProcessinInfoQueue(testSize * 3),
                TestDataManager.NewOrderProcessinInfoQueue(testSize * 2),
                TestDataManager.NewOrderProcessinInfoQueue(testSize));
            DataAccessor.OrderRepository = reprository;
            DataAccessor.NodeMonitor = reprository;

            
            var processingQueue = new BlockingCollection<OrderProcessingInfo>();

            List<NodeScheduler> schdulers = new List<NodeScheduler>();
            TestOrderProcessor processor = new TestOrderProcessor();
            for (int i = 0; i < 5; ++i)
            {
                schdulers.Add( new NodeScheduler("BVT",
                TestConfiguration(), processingQueue, processor));
            }

            ResetPerfCount();

            schdulers.ForEach(i=>i.Start());
            while (!reprository.IsEmpty)
            {
                Thread.Sleep(2000);
            }
            schdulers.ForEach(i=>i.Stop());

            Assert.AreEqual(totalSize, processor.PreProcessed);
            Assert.AreEqual(totalSize, processor.Processed);
            Assert.AreEqual(totalSize, processor.PostProcessed);

            Assert.AreEqual(totalSize, Statistic.Stat.ProcessCompleted);
            Assert.AreEqual(totalSize, Statistic.Stat.Processed);

        }

    }
}
