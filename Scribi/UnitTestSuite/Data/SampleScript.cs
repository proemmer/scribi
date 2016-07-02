using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [RestMethod("GET")]
        public string GetData()
        {
            _logger.LogInformation("GetData Called");
            return "Sample";
        }
    }
}
