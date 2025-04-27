using NewsApp.Models.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace NewsApp.Services.Helpers
{
    public class ResponseHelper
    {
        public ActionResult HandleResponse(ControllerBase controller, BaseResponse baseResponse)
        {
            if (controller.HttpContext.Request.Headers.TryGetValue("CorrelationId", out var value))
            {
                baseResponse.CorrelationId = value;
            };
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

        public BaseResponse HandleSuccess(object? data = null, string? message = "")
        {
            return new BaseResponse
            {
                StatusCode = HttpStatusCode.OK,
                Message = message,
                Data = data,
                Success = true
            };
        }

        public BaseResponse HandleConflict(string entityName)
        {
            return new BaseResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = entityName + " already exists.",
                Success = false
            };
        }

        public BaseResponse HandleNotFound(string entityName)
        {
            return new BaseResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = entityName + " not found.",
                Success = false
            };
        }

        public BaseResponse HandleBadRequest(string message)
        {
            return new BaseResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = message,
                Success = false
            };
        }

        public BaseResponse HandleDBInsertionResponse(string entity, object data, long? result)
        {
            if (result > 0)
            {
                return new BaseResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Data Added in " + entity + " ",
                    Data = data,
                    Success = true
                };
            }

            return new BaseResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = entity + " Insertion failed",
                Success = false
            };
        }

        public BaseResponse HandleDBUpdateResponse(string entity, object data, long? result)
        {
            if (result > 0)
            {
                return new BaseResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Data Updated in " + entity + " ",
                    Data = data,
                    Success = true
                };
            }

            return new BaseResponse
            {
                StatusCode = HttpStatusCode.ExpectationFailed,
                Message = entity + " Updation failed",
                Success = false
            };
        }

        public BaseResponse HandleDelete(string entity, int? rows)
        {
            if (rows > 0)
            {
                return new BaseResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Data Deleted in " + entity + " ",
                    Success = true
                };
            }

            return new BaseResponse
            {
                StatusCode = HttpStatusCode.ExpectationFailed,
                Message = entity + " Deletion failed",
                Success = false
            };
        }

        public BaseResponse HandleExceptionResponse(Exception ex)
        {
            return new BaseResponse
            {
                StatusCode = HttpStatusCode.ExpectationFailed,
                Message = "Exception Occurred: " + ex.Message,
                Success = false
            };
        }

        public BaseResponse HandleServerErrorResponse(Exception ex)
        {
            return new BaseResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = "Internal Server Error: " + ex.Message,
                // Data = ex,
                Success = false
            };
        }

        public BaseResponse HandleServiceUnavailableError(string message)
        {
            return new BaseResponse
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Message = (message ?? ""),
                Success = false
            };
        }

        public BaseResponse HandleAuthFailure()
        {
            return new BaseResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Invalid Access.",
                Data = null,
                Success = false
            };
        }

        public BaseResponse HandlePermissionFailure()
        {
            return new BaseResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "The user does not have the permission to perform the task",
                Data = null,
                Success = false
            };
        }
    }
}

