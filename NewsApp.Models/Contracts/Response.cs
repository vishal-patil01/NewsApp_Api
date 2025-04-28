
using System.Net;

namespace NewsApp.Models.Contracts
{
    public class Response
    {
        public string Message { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
        public object Data { get; set; } = false;
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    }
}
