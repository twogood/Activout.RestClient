using System;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public class RouteAttribute : TemplateAttribute
    {
        public RouteAttribute(string template) : base(template)
        {
        }
    }
}