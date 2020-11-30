using System;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public class PathAttribute : TemplateAttribute
    {
        public PathAttribute(string template) : base(template)
        {
        }
    }
}