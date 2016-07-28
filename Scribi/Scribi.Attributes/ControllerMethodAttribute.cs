using System;

namespace Scribi.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ControllerMethodAttribute : Attribute
    {
        public string HttpMethod { get; set; }
        public string RouteTemplate { get; set; }
        public string Policy { get; set; }
        public ControllerMethodAttribute(string httpMethod, string routeTemplate = "", string policy = "")
        {
            HttpMethod = httpMethod;
            RouteTemplate = routeTemplate;
            Policy = policy;
        }
    }
}
