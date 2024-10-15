using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Quartz;
using QuartzScheduler.Common.Exceptions;

namespace QuartzScheduler.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Handling request error");

            var statusCode = GenerateStatusCode(context.Exception);
            var message = statusCode == HttpStatusCode.InternalServerError
                ? "An unhandled error occurred"
                : context.Exception.GetBaseException().Message;
            var stack = string.Empty;

#if DEBUG
            stack = context.Exception.StackTrace;
#endif

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)statusCode;

            context.Result = string.IsNullOrWhiteSpace(stack)
                ? new JsonResult(new { error = message })
                : new JsonResult(new
                {
                    error = message,
                    stackTrace = stack
                });

            base.OnException(context);
        }

        private static HttpStatusCode GenerateStatusCode(Exception exception) => exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            AlreadyExistsException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            SchedulerException when exception.Message.Contains("will never fire") => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };
    }
}
