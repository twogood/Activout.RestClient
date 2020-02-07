using System;
using System.Net.Http;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class HttpMethodAttribute : TemplateAttribute
    {
        public HttpMethod HttpMethod { get; }

        protected HttpMethodAttribute(HttpMethod httpMethod, string template) : base(template)
        {
            HttpMethod = httpMethod;
        }
    }

    public class HttpDeleteAttribute : HttpMethodAttribute
    {
        public HttpDeleteAttribute(string template = null) : base(HttpMethod.Delete, template)
        {
        }
    }

    public class HttpGetAttribute : HttpMethodAttribute
    {
        public HttpGetAttribute(string template = null) : base(HttpMethod.Get, template)
        {
        }
    }


    public class HttpPostAttribute : HttpMethodAttribute
    {
        public HttpPostAttribute(string template = null) : base(HttpMethod.Post, template)
        {
        }
    }


    public class HttpPutAttribute : HttpMethodAttribute
    {
        public HttpPutAttribute(string template = null) : base(HttpMethod.Put, template)
        {
        }
    }
}