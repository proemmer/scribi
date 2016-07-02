using System;

namespace Scribi.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RestMethodAttribute : Attribute
    {
        public string HttpMethod { get; set; }
        public string RouteTemplate { get; set; }
        public RestMethodAttribute(string httpMethod, string routeTemplate = "")
        {
            HttpMethod = httpMethod;
            RouteTemplate = routeTemplate;
        }
    }
}
