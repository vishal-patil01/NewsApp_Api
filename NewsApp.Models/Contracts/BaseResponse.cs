
using System.Net;

namespace NewsApp.Models.Contracts
{
    public class BaseResponse
    {
        public virtual string Message { get; set; } = string.Empty;
        public virtual string CorrelationId { get; set; } = string.Empty;
        public virtual bool Success { get; set; } = false;
        public virtual object Data { get; set; } = false;
        public virtual HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    }
}
