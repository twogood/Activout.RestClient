#nullable disable
using System;

namespace Activout.RestClient
{
    [Obsolete("Please use [Path] instead!")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public class RouteAttribute : PathAttribute
    {
        public RouteAttribute(string template) : base(template)
        {
        }
    }
}