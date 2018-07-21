using System;
using Microsoft.AspNetCore.Mvc;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FormParamAttribute : FromFormAttribute
    {
        public FormParamAttribute()
        {
            // deliberately empty    
        }
        
        public FormParamAttribute(string name)
        {
            Name = name;
        }
    }
}