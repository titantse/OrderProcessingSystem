
namespace OrderProcessing.Common
{
    using System;
    using OrderProcessing.Domain;
    /// <summary>
    /// Syntax sugar for some common operations.
    /// </summary>
    public static class Extentions
    {
        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static T CheckGreaterThan<T>(this T left, T right, string valueDescription) where T : IComparable<T>
        {
            if (left.CompareTo(right) <= 0)
            {
                throw new OrderProcessingException(
                    ErrorCode.InvalidValue,
                    string.Format("Value:{0} should be greater than {1}",
                        valueDescription, right));
            }
            return left;
        }
    }
}