using System;

namespace Activout.RestClient
{
    [Obsolete("Please use [PathParam] instead!")]
    public class RouteParamAttribute : PathParamAttribute
    {
        public RouteParamAttribute(string name = null) : base(name)
        {
        }
    }
}