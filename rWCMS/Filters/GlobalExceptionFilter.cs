using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace rWCMS.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var statusCode = context.Exception switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var message = context.Exception switch
            {
                UnauthorizedAccessException => context.Exception.Message,
                ArgumentException => context.Exception.Message,
                _ => "An unexpected error occurred."
            };

            context.Result = new ObjectResult(new 
            {
                error = message,
                stackTrace = context.Exception.StackTrace
            })
            {
                StatusCode = statusCode
            };
            context.ExceptionHandled = true;
        }
    }
}