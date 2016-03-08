
namespace OrderProcessing.Common
{
    using System.Net;

    /// <summary>
    /// System related utility.
    /// </summary>
    public static class SystemUtility
    {
        /// <summary>
        /// Get local IP of the machine.
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }
    }
}
