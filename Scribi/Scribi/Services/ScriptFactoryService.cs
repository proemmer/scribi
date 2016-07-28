using Scribi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Scribi.Attributes;
using Scribi.Model;
using Microsoft.Extensions.Logging;

namespace Scribi.Services
{
    public class ScriptFactoryService : IScriptFactoryService
    {
        private ILogger<ScriptFactoryService> _logger;
        private IScriptCreatorService _scriptCreator;
        private ICyclicExecutorService _cycleExecutor;
        private List<FunctionCall> _functionCalls = new List<FunctionCall>();
        private class FunctionCall
        {
            public Type ScriptType { get; set; }
            public FunctionCallAttribute FunctionCallConfig { get; set; }
            public MethodInfo Method { get; set; }
        }


        public IEnumerable<string> GetScriptNames()
        {
            return _scriptCreator.Scripts.Select(x => x?.GetTypeInfo()?.GetCustomAttribute<ScriptUnitAttribute>()?.Name);
        }

        public ScriptMetaData GetScriptMetaInfo(string script)
        {
            var scriptType = _scriptCreator.Scripts.FirstOrDefault(x => x?.GetTypeInfo()?.GetCustomAttribute<ScriptUnitAttribute>()?.Name == script);
            return scriptType != null ? new ScriptMetaData
            {
                Name = scriptType.Name,
                Methods = scriptType.GetMethods(BindingFlags.Public).Select(x => x.Name),
                Properties = scriptType.GetProperties(BindingFlags.Public).Select(x => x.Name),
                LifecycleType = scriptType.GetTypeInfo().GetCustomAttribute<ScriptUnitAttribute>().LifecycleType
            }
            : null;
        }


        public ScriptFactoryService(ILogger<ScriptFactoryService> logger, IScriptCreatorService scriptCreator, ICyclicExecutorService cycleExecutor)
        {
            _scriptCreator = scriptCreator;
            _cycleExecutor = cycleExecutor;
            _logger = logger;
        }

        public void Configure(IConfigurationSection config)
        {
            foreach (var item in _scriptCreator.Scripts)
            {
                foreach (var methodInfo in item.GetMethods())
                {
                    var att = methodInfo.GetCustomAttributes(typeof(FunctionCallAttribute), false);
                    if (!att.Any()) continue;

                    foreach (var attribute in att.OfType<FunctionCallAttribute>())
                    {
                        _functionCalls.Add(new FunctionCall
                        {
                            ScriptType = item,
                            FunctionCallConfig = attribute,
                            Method = methodInfo
                        });
                    }
                }
            }
        }

        public void Init()
        {
            InvokeFunctionCallMethods(FunctionCallType.CallOnStart);

            foreach (var item in _functionCalls.Where(x => x.FunctionCallConfig.GetFunctionCallType() == FunctionCallType.CallOnDelay))
            {
                var name = item.ScriptType.Name + item.Method.Name + "DELAY";
                _cycleExecutor.Add(name, name, item.FunctionCallConfig.TimeMs, () => InvokeFunctionCall(item), true, false, true);
            }

            RegisterTimeAndIntervallScripts();
        }

        public void Release()
        {
            InvokeFunctionCallMethods(FunctionCallType.CallOnStop);
        }


        private void RegisterTimeAndIntervallScripts()
        {
            foreach (var item in _functionCalls.Where(x => x.FunctionCallConfig.GetFunctionCallType() == FunctionCallType.CallOnInterval))
            {
                var name = item.ScriptType.Name + item.Method.Name + "INTERVAL";
                _cycleExecutor.Add(name, name, item.FunctionCallConfig.TimeMs, () => InvokeFunctionCall(item), true);
            }

            foreach (var item in _functionCalls.Where(x => x.FunctionCallConfig.GetFunctionCallType() == FunctionCallType.CallOnTime))
            {
                var name = item.ScriptType.Name + item.Method.Name + "TIME";
                _cycleExecutor.Add(name, name, CalcSleepTime(item.FunctionCallConfig.DateTime).TotalMilliseconds, () =>
                {
                    _cycleExecutor.Enabled(name, false);
                    InvokeFunctionCall(item);
                    _cycleExecutor.Update(name, CalcSleepTime(item.FunctionCallConfig.DateTime).TotalMilliseconds);
                    _cycleExecutor.Enabled(name, false);
                }, true);
            }
        }

        private void InvokeFunctionCallMethods(FunctionCallType functionCallType)
        {
            foreach (var item in _functionCalls.Where(x => x.FunctionCallConfig.GetFunctionCallType() == functionCallType))
            {
                InvokeFunctionCall(item);
            }
        }

        private static TimeSpan CalcSleepTime(DateTime dt)
        {
            return DateTime.Now.TimeOfDay < dt.TimeOfDay ? dt.TimeOfDay - DateTime.Now.TimeOfDay : dt.TimeOfDay.Add(TimeSpan.FromDays(1)) - DateTime.Now.TimeOfDay;
        }

        private void InvokeFunctionCall(FunctionCall item)
        {
            try
            {
                var script = _scriptCreator.ServiceProvider.GetService(item.ScriptType);
                if (script != null)
                    item.Method.Invoke(script, null);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Exception while invoking method {item.Method.Name} of script {item.ScriptType}. Error was: {ex.Message}");
            }
        }
    }
}
