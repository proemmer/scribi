using Scribi.Attributes;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scribi.CodeGeneration
{
    internal static class ControllerCreator
    {
        #region Controller Creation
        private const string ControllerTemplate =
        @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Scribi.Interfaces;
using Scribi.Filters;


namespace Scribi.Controllers
{{
    [ServiceFilter(typeof(ActionLoggerFilterAttribute))]
    [ServiceFilter(typeof(ScribiExceptionFilterAttribute))]
    [Route(""api/[controller]"")]
    public class {0}Controller : Controller
    {{
        private readonly {2} _obj;

        public {0}Controller(IScriptCreatorService ccs)
        {{
            //To get the selfe registred services, because asp.net 
            //service provider did not update at runtime
            _obj = ccs.ServiceProvider.GetRequiredService(typeof({2})) as {2};
        }}

{1}
    }}
}}
        ";

        private const string ControllerMethod =
        @"
        {0}
        {5}
        public {1} {2} ({3})
        {{
            return _obj.{2}({4});
        }}
        ";
        #endregion


        public static string Create(Type type, ScriptUnitAttribute attr)
        {
            var sb = new StringBuilder();
            var methods = type.GetMethods();
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes<ControllerMethodAttribute>();
                if (attributes.Any())
                {
                    foreach (var attribute in attributes)
                    {
                        var parameters = method.GetParameters();
                        sb.Append(string.Format(ControllerMethod,
                                                HttpMethodToAttribute(attribute),
                                                method.ReturnType,
                                                method.Name,
                                                parameters.Any() ? ParametersToParameters(parameters) : string.Empty,
                                                parameters.Any() ? ParametersToCallParams(parameters) : string.Empty,
                                                string.IsNullOrWhiteSpace(attribute.Policy) ? "[AllowAnonymous]" : $"[Authorize(Policy = \"{attribute.Policy}\")]"));
                        sb.AppendLine();
                    }
                }
            }
            return string.Format(ControllerTemplate, attr.Name, sb.ToString(), type);
        }


        private static string HttpMethodToAttribute(ControllerMethodAttribute attr)
        {
            switch (attr.HttpMethod.ToUpper())
            {
                case "GET":
                    return string.IsNullOrWhiteSpace(attr.RouteTemplate) ? "[HttpGet]" : $"[HttpGet(\"{attr.RouteTemplate}\")]";
                case "POST":
                    return string.IsNullOrWhiteSpace(attr.RouteTemplate) ? "[HttpPost]" : $"[HttpPost(\"{attr.RouteTemplate}\")]";
                case "PATCH":
                    return string.IsNullOrWhiteSpace(attr.RouteTemplate) ? "[HttpPatch]" : $"[HttpPatch(\"{attr.RouteTemplate}\")]";
                case "PUT":
                    return string.IsNullOrWhiteSpace(attr.RouteTemplate) ? "[HttpPut]" : $"[HttpPut(\"{attr.RouteTemplate}\")]";
                case "DELETE":
                    return string.IsNullOrWhiteSpace(attr.RouteTemplate) ? "[HttpPut]" : $"[HttpPut(\"{attr.RouteTemplate}\")]";
            }
            return string.Empty;
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
