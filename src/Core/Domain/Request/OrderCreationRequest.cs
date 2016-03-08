
namespace OrderProcessing.Domain.Request
{
    /// <summary>
    ///     This is an order request submitted by user.
    ///     In realworld, there should have include more information.
    ///     Here uses a simple string for details.
    /// </summary>
    public class OrderCreationRequest
    {
        /// <summary>
        ///     This is used for the idempotency so client could submit a same request repeatedly.
        /// </summary>
        public string TrackingId { get; set; }
        /// <summary>
        /// In real world, there should be more than just one column Detail.
        /// </summary>
        public string OrderDetail { get; set; }
    }
}