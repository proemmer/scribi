using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Scribi.Filters
{
    public class ScribiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        public ScribiExceptionFilterAttribute(ILogger<ScribiExceptionFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError($"OnException: {context.Exception.Message} - {context.Exception.StackTrace}");
            base.OnException(context);
        }
    }
}
