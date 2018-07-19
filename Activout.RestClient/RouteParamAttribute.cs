using System;
using Microsoft.AspNetCore.Mvc;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RouteParamAttribute : FromRouteAttribute
    {
        public RouteParamAttribute(string name)
        {
            Name = name;
        }
    }
}