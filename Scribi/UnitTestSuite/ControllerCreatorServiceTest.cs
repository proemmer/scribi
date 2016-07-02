using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scribi.Services;
using Xunit;

namespace UnitTestSuite
{
    public class ControllerCreatorServiceTest
    {
        
        public ControllerCreatorServiceTest()
        {
        }

        [Fact]
        public void ExtractControllersFromTypesTest()
        {
            //var mockLoggerComp = new Mock<ILogger<RuntimeCompilerService>>();
            //var mockLogger = new Mock<ILogger<ControllerCreatorService>>();
            //var runtimeCompiler = new RuntimeCompilerService(mockLoggerComp.Object);
            //var controllerCreatorService = new ControllerCreatorService(mockLogger.Object, runtimeCompiler);

            //runtimeCompiler.Init();
            //var result = controllerCreatorService.ExtractControllersFromTypes(new List<Type> { typeof(SampleScript) });

            //Assert.True(result.Any());

            //runtimeCompiler.CompileFiles(result, "Controllers");



        }

    }
}
