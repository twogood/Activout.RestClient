using System;
using Microsoft.AspNetCore.Mvc;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class QueryParamAttribute : FromQueryAttribute
    {
        public QueryParamAttribute()
        {
            // deliberately empty    
        }
        
        public QueryParamAttribute(string name)
        {
            Name = name;
        }
    }
}