using System;

namespace Scribi.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ControllerMethodAttribute : Attribute
    {
        public string HttpMethod { get; set; }
        public string RouteTemplate { get; set; }
        public ControllerMethodAttribute(string httpMethod, string routeTemplate = "")
        {
            HttpMethod = httpMethod;
            RouteTemplate = routeTemplate;
        }
    }
}
