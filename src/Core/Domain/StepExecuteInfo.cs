
namespace OrderProcessing.Domain
{
    using System;

    /// <summary>
    /// Step execute info.
    /// </summary>
    public class StepExecuteInfo
    {
        public bool Success { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? CompleteTime { get; set; }
    }
}