
namespace OrderProcessing.Common
{
    using System;
    using System.Text;
    /// <summary>
    /// The ID generator for an order request, we use a GUID here.
    /// </summary>
    public static class IDGenerator
    {
        public static string NewOrderId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}