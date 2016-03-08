
namespace OrderProcessing.WorkNode
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using OrderProcessing.Domain;

    /// <summary>
    /// This control provided a way to communicate with the working node.
    /// </summary>
    public class CommandController : ApiController
    {
        /// <summary>
        /// Stop the progrram.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Stop()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5*1000);
                Program.Stop();
            });
            return Request.CreateResponse(HttpStatusCode.OK, "Stopping the service.", "application/json");
        }

        /// <summary>
        /// View the statistics
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Stat()
        {
            return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(Statistic.Stat), "application/json");
        }
    }
}
