using System;
using System.Net.Http;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class HttpMethodAttribute : TemplateAttribute
    {
        public HttpMethod HttpMethod { get; }

        protected HttpMethodAttribute(HttpMethod httpMethod, string template) : base(template)
        {
            HttpMethod = httpMethod;
        }
    }

    public class DeleteAttribute : HttpMethodAttribute
    {
        public DeleteAttribute(string template = null) : base(HttpMethod.Delete, template)
        {
        }
    }

    public class GetAttribute : HttpMethodAttribute
    {
        public GetAttribute(string template = null) : base(HttpMethod.Get, template)
        {
        }
    }


    public class PostAttribute : HttpMethodAttribute
    {
        public PostAttribute(string template = null) : base(HttpMethod.Post, template)
        {
        }
    }


    public class PutAttribute : HttpMethodAttribute
    {
        public PutAttribute(string template = null) : base(HttpMethod.Put, template)
        {
        }
    }
}