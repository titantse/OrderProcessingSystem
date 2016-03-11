
namespace OrderProcessing.Test.UT
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using OrderProcessing.Configuration;
    using OrderProcessing.Domain;

    [TestClass]
    public class TestBasic
    {
        /// <summary>
        /// Confiugration test.
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void Configuration()
        {
            var section = WorkNodeConfiguration.Current;
            Assert.IsNotNull(section);
            Assert.AreEqual(1000, section.Scheduler.MaxQueueLength);
            Assert.AreEqual(10, section.WorkNode.MaxConcurrentWorkingThreads);
            Assert.AreEqual(5, section.Monitor.HeartBeatIntervalSeconds);
            Assert.AreEqual(30, section.Monitor.MaxNoHeartBeatIntervalSeconds);
            Assert.AreEqual(2, section.Scheduler.PullingTasksIntervalSeconds);
            Assert.AreEqual(30, section.Scheduler.PullingTasksEachTime);
        }

        /// <summary>
        /// Json serialize and deserialize
        /// </summary>
        [TestMethod]
        [TestCategory("BVT")]
        public void StepInfoSeriliazeAndDeserilize()
        {
            var executeTime = DateTime.Today;
            var steps = new Dictionary<OrderStatus, StepExecuteInfo>();
            steps.Add(OrderStatus.Scheduling, new StepExecuteInfo { StartTime = executeTime, CompleteTime = executeTime });
            steps.Add(OrderStatus.PreProcessing,
                new StepExecuteInfo { StartTime = executeTime, CompleteTime = executeTime });
            steps.Add(OrderStatus.Processing, new StepExecuteInfo { StartTime = executeTime });
            steps.Add(OrderStatus.PostProcessing, new StepExecuteInfo());
            var str = JsonConvert.SerializeObject(steps);
            Console.WriteLine(str);
            var steps2 =
                JsonConvert.DeserializeObject<Dictionary<OrderStatus, StepExecuteInfo>>(str);
            Assert.IsNotNull(steps2);
            Assert.AreEqual(steps.Count, steps2.Count);
            Assert.AreEqual(executeTime, steps2[OrderStatus.Scheduling].StartTime.Value);
            Assert.AreEqual(executeTime, steps2[OrderStatus.PreProcessing].CompleteTime.Value);
            Assert.AreEqual(executeTime, steps2[OrderStatus.Processing].StartTime.Value);
            Assert.IsNull(steps2[OrderStatus.Processing].CompleteTime);
            Assert.IsNotNull(steps[OrderStatus.PostProcessing]);
            Assert.IsNull(steps[OrderStatus.PostProcessing].StartTime);
            Assert.IsNull(steps[OrderStatus.PostProcessing].CompleteTime);
        }
    }
}