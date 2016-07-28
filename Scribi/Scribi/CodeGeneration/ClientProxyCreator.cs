using Scribi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scribi.CodeGeneration
{
    /// <summary>
    /// TODO How to call Signal R  Client methods from user code!!!???
    /// </summary>
    internal static class ClientProxyCreator
    {
        #region Controller Creation
        private const string HubTemplate =
        @"
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Threading.Tasks;
        using Microsoft.AspNetCore.SignalR;
        using Microsoft.AspNetCore.SignalR.Hubs;
        using Microsoft.Extensions.DependencyInjection;
        using Scribi.Interfaces;

        namespace Scribi.ProxyClient
        {{
            public class {0}ProxyClient
            {{
                private readonly {2} _obj;

                public {0}ProxyClient(IScriptCreatorService ccs)
                {{
                    //To get the selfe registred services, because asp.net 
                    //service provider did not update at runtime
                    _obj = ccs.ServiceProvider.GetRequiredService(typeof({2})) as {2};
                }}

        {1}
            }}
        }}
        ";

        private const string HubMethod =
        @"
        public {0} {1} ({2})
        {{
            return _obj.{1}({3});
        }}
        ";
        #endregion


        public static string Create(Type tHub, Type tClient)
        {
            var sb = new StringBuilder();
            var methods = tClient.GetMethods();
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                sb.Append(string.Format(HubMethod,
                                        method.ReturnType,
                                        method.Name,
                                        parameters.Any() ? ParametersToParameters(parameters) : string.Empty,
                                        parameters.Any() ? ParametersToCallParams(parameters) : string.Empty));
                sb.AppendLine();
            }

            var ifType = typeof(IClientWrapper<>).MakeGenericType(tClient);
            var instanceType = typeof(ClientWrapper<>).MakeGenericType(tClient);
            return string.Format(HubTemplate, tHub.Name, sb.ToString(), ifType, ifType);
        }

        private static string ParametersToParameters(ParameterInfo[] parameters)
        {
            var sb = new StringBuilder();
            foreach (var param in parameters)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(param.ParameterType);
                sb.Append(" ");
                sb.Append(param.Name);
            }
            return sb.ToString();
        }

        private static string ParametersToCallParams(ParameterInfo[] parameters)
        {
            var sb = new StringBuilder();
            foreach (var param in parameters)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(param.Name);
            }
            return sb.ToString();
        }
    }
}

