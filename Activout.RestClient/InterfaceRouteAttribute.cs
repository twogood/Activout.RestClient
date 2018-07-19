using System;
using Microsoft.AspNetCore.Mvc;

namespace Activout.RestClient
{
    // Because RouteAttribute is not valid on interfaces!
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public class InterfaceRouteAttribute : RouteAttribute
    {
        public InterfaceRouteAttribute(string template) :
            base(template)
        {
        }
    }
}