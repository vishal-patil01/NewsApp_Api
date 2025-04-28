using System.Net;
using Microsoft.AspNetCore.Mvc;
using NewsApp.Models.Contracts;

namespace NewsApp.Services.Helpers
{
    public class ResponseHelper
    {
        public ActionResult HandleResponse(ControllerBase controller, Response baseResponse)
        {
            if (controller.HttpContext.Request.Headers.TryGetValue("CorrelationId", out Microsoft.Extensions.Primitives.StringValues value))
            {
                baseResponse.CorrelationId = value;
            }
            ;
            baseResponse.CorrelationId = Guid.NewGuid().ToString();
            return baseResponse.StatusCode switch
            {
                HttpStatusCode.OK => controller.Ok(baseResponse),
                HttpStatusCode.NotFound => controller.NotFound(baseResponse),
                HttpStatusCode.BadRequest => controller.BadRequest(baseResponse),
                HttpStatusCode.Conflict => controller.Conflict(baseResponse),
                HttpStatusCode.Unauthorized => controller.Unauthorized(),
                _ => controller.StatusCode((int)baseResponse.StatusCode, baseResponse),
            };
        }

        public Response HandleSuccess(object? data = null, string? message = "")
        {
            return new Response
            {
                StatusCode = HttpStatusCode.OK,
                Message = message,
                Data = data,
                Success = true
            };
        }

        public Response HandleNotFound(string entityName)
        {
            return new Response
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = entityName + " not found.",
                Success = false
            };
        }

    }
}

