using Microsoft.AspNetCore.Mvc;
using System;

namespace Activout.RestClient
{
    // Because RouteAttribute is not valid on interfaces!
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class InterfaceRouteAttribute : RouteAttribute
    {
        public InterfaceRouteAttribute(string template) :
            base(template)
        {
        }
    }
}
