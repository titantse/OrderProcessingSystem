

namespace OrderProcessing.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public class ErrorCode
    {
        private static readonly Dictionary<string, ErrorCode> SqlErrorMapping;

        public static ErrorCode InternalError = new ErrorCode(true, 1, "Internal server error happened");

        public static ErrorCode InvalidRequest = new ErrorCode(false, 1000, "The request is invalid");

        public static ErrorCode InvalidOrderId = new ErrorCode(false, 1001, "Invalid Order Id");

        public static ErrorCode OrderNotExist = new ErrorCode(false, 1002, "Order does not exist");

        public static ErrorCode NullReference = new ErrorCode(true, 1003,
            "The object reference should not be null in this context");

        public static ErrorCode InvalidOrdertatus = new ErrorCode(false, 1004, "The target order status is invalid");

        public static ErrorCode InvalidProperty = new ErrorCode(false, 1005, "One of data field is not valid");

        public static ErrorCode InvalidValue = new ErrorCode(false, 1006, "The input value is not not expected");

        public static ErrorCode TimestampConflict = new ErrorCode(false, 2000,
            "The timpstamp does not match, someone has changed the data");

        static ErrorCode()
        {
            SqlErrorMapping = new Dictionary<string, ErrorCode>(StringComparer.OrdinalIgnoreCase)
            {
                {"ORDER_NOT_EXIST", OrderNotExist},
                {"ORDER_ID_AREADY_EXISTS", InvalidOrderId},
                {"TIMESTAMP_CONFLICT", TimestampConflict}
            };
        }

        public ErrorCode()
        {
        }

        private ErrorCode(bool isServerError, int code, string description)
        {
            Code = code;
            Description = description;
            IsServerError = isServerError;
        }

        public int Code { get; private set; }

        public string Description { get; private set; }

        public bool IsServerError { get; private set; }

        public static ErrorCode MappingSqlError(SqlException se)
        {
            return SqlErrorMapping.ContainsKey(se.Errors[0].Message)
                ? SqlErrorMapping[se.Errors[0].Message]
                : InternalError;
        }
    }
}