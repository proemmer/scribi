using Microsoft.AspNetCore.Mvc.Filters;
using System.IO;
using System.IO.Compression;

namespace Scribi.Filters
{
    public class CompressFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// http://www.erwinvandervalk.net/2015/02/enabling-gzip-compression-in-webapi-and.html
        /// https://minhtuanq6.wordpress.com/2016/02/17/asp-net-web-api-gzip-compression-actionfilter/
        /// http://blog.developers.ba/asp-net-web-api-gzip-compression-actionfilter/
        /// 
        /// https://github.com/pmachowski/angular2-starter-kit
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            string acceptEncoding = request.Headers["Accept-Encoding"];

            if (string.IsNullOrEmpty(acceptEncoding)) return;

            acceptEncoding = acceptEncoding.ToUpperInvariant();

            var response = context.HttpContext.Response;
            var responseStream = response.Body;
            if (acceptEncoding.Contains("GZIP"))
            {
                response.Headers.Add("Content-Encoding", new[] { "gzip" });
                response.Body = new GZipStream(responseStream, CompressionMode.Compress);
                response.Body.Flush();
            }
            else if (acceptEncoding.Contains("DEFLATE"))
            {
                response.Headers.Add("Content-Encoding", new[] { "deflate" });
                response.Body = new DeflateStream(responseStream, CompressionMode.Compress);
                response.Body.Flush();
            }

            base.OnActionExecuting(context);
        }
    }
}
