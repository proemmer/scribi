using Scribi.Attributes;
using Microsoft.Extensions.Logging;
using Scribi.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTestSuite
{

    //Create a wrapper class which will be injected an do the call
    //This is the interfaces for the client methods of the SignalRHub
    public interface ISampleScriptClient
    {
        bool CallClient(string test);
    }

    [ScriptUnit("Sample",AccessType.Remote, typeof(UnitTestSuite.ISampleScriptClient))]
    public class SampleScript
    {
        private readonly ILogger _logger;
        private readonly IClientWrapper<ISampleScriptClient> _client;



        public SampleScript(ILogger<SampleScript> logger, IScriptCreatorService ccs)
        {
            _logger = logger;
            _client = ccs.ServiceProvider.GetRequiredService(typeof(IClientWrapper<ISampleScriptClient>)) as IClientWrapper<ISampleScriptClient>;
        }

        [ControllerMethod("GET")]
        public string GetData()
        {
            _logger.LogInformation("GetData Called");
            return "Sample";
        }

        public bool CallClient()
        {
            _logger.LogInformation("Call Client");
            return _client.All.CallClient("TEST");
        }

        [HubMethod(HubMethodType.Client)]
        private void SignalClientCallServer()
        {
            _logger.LogInformation("SignalR Client calls server");
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
