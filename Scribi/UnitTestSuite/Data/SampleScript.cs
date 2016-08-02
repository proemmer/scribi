using Scribi.Attributes;
using Microsoft.Extensions.Logging;
using Scribi.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace UnitTestSuite
{

    //Create a wrapper class which will be injected an do the call
    //This is the interfaces for the client methods of the SignalRHub
    public interface ISampleScriptClient
    {
        void CallClient(string test);
    }

    [ScriptUnit("Sample",AccessType.Remote, typeof(UnitTestSuite.ISampleScriptClient))]
    //[ScriptUnit("Sample", AccessType.Rest)]
    public class SampleScript
    {
        private readonly ILogger _logger;
        private readonly IClientProxy<ISampleScriptClient> _client;



        public SampleScript(ILogger<SampleScript> logger, IClientProxy<ISampleScriptClient> client)
        {
            _logger = logger;

            try
            {
                _client = client;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        [ControllerMethod("GET")]
        public string GetData()
        {
            _logger.LogInformation("GetData Called");
            return "Sample";
        }

        public void CallClient()
        {
            _logger.LogInformation("Call Client");
            _client.All.CallClient("TEST");
        }

        [HubMethod(HubMethodType.Client)]
        public void SignalClientCallServer()
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
            try
            {
                CallClient();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            
        }

        [FunctionCall(FunctionCallType.CallOnInterval, 10000)]
        public void CallOnInterval()
        {
            _logger.LogInformation("CallOnInterval");
        }

    }
}
