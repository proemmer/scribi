using Scribi.Attributes;
using Microsoft.Extensions.Logging;

namespace UnitTestSuite
{
    [ScriptUnit("Sample",AccessType.Rest)]
    public class SampleScript
    {

        private readonly ILogger _logger;

        public SampleScript(ILogger<SampleScript> logger)
        {
            _logger = logger;
        }

        [ControllerMethod("GET")]
        public string GetData()
        {
            _logger.LogInformation("GetData Called");
            return "Sample";
        }


        [FunctionCall(FunctionCallType.CallOnStart)]
        public void CallOnStart()
        {
            _logger.LogInformation("CallOnStart");
        }

        [FunctionCall(FunctionCallType.CallOnDelay,5000)]
        public void CallDelay()
        {
            _logger.LogInformation("CallDelay");
        }

        [FunctionCall(FunctionCallType.CallOnInterval, 10000)]
        public void CallOnInterval()
        {
            _logger.LogInformation("CallOnInterval");
        }

    }
}
