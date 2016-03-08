
namespace OrderProcessing.Domain
{
    using System;

    /// <summary>
    /// The Exception of OrderProcessingSystem.
    /// </summary>
    public class OrderProcessingException : Exception
    {
        public OrderProcessingException(ErrorCode errCode)
            : this(errCode, string.Empty)
        {
        }

        public OrderProcessingException(ErrorCode errCode, string message)
            : base(message, null)
        {
            ErrorCode = errCode;
        }

        public OrderProcessingException(ErrorCode errCode, Exception innerException)
            : base(errCode.Description, innerException)
        {
            ErrorCode = errCode;
        }

        public ErrorCode ErrorCode { get; private set; }
    }
}