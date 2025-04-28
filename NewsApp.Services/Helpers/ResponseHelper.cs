using System.Net;
using Microsoft.AspNetCore.Mvc;
using NewsApp.Models.Contracts;

namespace NewsApp.Services.Helpers
{
    /// <summary>
    /// A helper class to standardize HTTP responses based on API actions.
    /// </summary>
    public class ResponseHelper
    {
        /// <summary>
        /// Processes a base response and returns the appropriate ActionResult.
        /// </summary>
        /// <param name="controller">The controller from which this is being called to build the ActionResult.</param>
        /// <param name="baseResponse">The standardized response containing data, status, and metadata.</param>
        /// <returns>An IActionResult generated based on the response's status code.</returns>
        public ActionResult HandleResponse(ControllerBase controller, Response baseResponse)
        {
            // Attempt to get a CorrelationId from the request headers, if present.
            if (controller.HttpContext.Request.Headers.TryGetValue("CorrelationId", out Microsoft.Extensions.Primitives.StringValues value))
            {
                baseResponse.CorrelationId = value;
            }
            // If no valid CorrelationId is retrieved, always generate a new unique identifier.
            baseResponse.CorrelationId = Guid.NewGuid().ToString();

            // Return the response based on the HTTP status code using a switch expression.

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

        /// <summary>
        /// Creates a success response.
        /// </summary>
        /// <param name="data">Optional data to be included in the response.</param>
        /// <param name="message">A descriptive message related to the success state.</param>
        /// <returns>A Response object indicating a successful operation.</returns>
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

        /// <summary>
        /// Creates a response indicating an entity was not found.
        /// </summary>
        /// <param name="entityName">The name of the entity that was not found.</param>
        /// <returns>A Response object indicating a not found error.</returns>
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

