using System;
using Microsoft.AspNetCore.Mvc;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class HeaderParamAttribute : FromHeaderAttribute
    {
        public HeaderParamAttribute()
        {
            // deliberately empty    
        }
        
        public HeaderParamAttribute(string name)
        {
            Name = name;
        }
    }
}