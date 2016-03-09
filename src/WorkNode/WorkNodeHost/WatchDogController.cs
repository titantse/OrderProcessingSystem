
namespace OrderProcessing.WorkNode
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    public class WatchDogController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Selfclosure()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "hello");
        }

        [HttpGet]
        public HttpResponseMessage External()
        {
            DataAccessor.DataAccessor.OrderRepository.Ping();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}