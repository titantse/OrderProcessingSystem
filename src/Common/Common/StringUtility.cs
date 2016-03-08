
namespace OrderProcessing.Common
{
    using System;
    using System.Text;

    /// <summary>
    /// String related utility.
    /// </summary>
    public static class StringUtility
    {
        /// <summary>
        /// Generate a randome string with length.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomNumberString(int length)
        {
            var r = new Random((int) DateTime.Now.Ticks);
            var sb = new StringBuilder();
            for (var i = 0; i < length; ++i)
            {
                sb.Append(r.Next(10) + '0');
            }
            return sb.ToString();
        }
    }
}