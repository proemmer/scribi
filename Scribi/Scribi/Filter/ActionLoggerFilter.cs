using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Scribi.Filters
{
    /// <summary>
    /// https://damienbod.com/2015/09/15/asp-net-5-action-filters/
    /// This Filter is used to log all Actions on the controllers
    /// </summary>
    public class ActionLoggerFilterAttribute : ActionFilterAttribute
    {
        private ILogger _logger;

        public ActionLoggerFilterAttribute(ILogger<ActionLoggerFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("OnActionExecuting");
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("OnActionExecuted");
            base.OnActionExecuted(context);
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            _logger.LogInformation("OnResultExecuting");
            base.OnResultExecuting(context);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            _logger.LogInformation("OnResultExecuted");
            base.OnResultExecuted(context);
        }

    }
}
