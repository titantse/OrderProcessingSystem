
namespace OrderProcessing.Interface
{
    /// <summary>
    /// INodeMonitor is used for self report the health of a working node.
    /// </summary>
    public interface INodeMonitor
    {
        /// <summary>
        /// Report about start.
        /// </summary>
        /// <param name="nodeId"></param>
        void ReportNodeStarted(string nodeId);

        /// <summary>
        /// Report heatbeat.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="currentQueueSize"></param>
        void ReportNodeHeartBeat(string nodeId, int availableThreads);
    }
}