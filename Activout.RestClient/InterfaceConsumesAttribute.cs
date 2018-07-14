using Microsoft.AspNetCore.Mvc;
using System;

namespace Activout.RestClient
{
    // Because ConsumesAttribute is not valid on interfaces
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class InterfaceConsumesAttribute : ConsumesAttribute
    {
        public InterfaceConsumesAttribute(string contentType, params string[] additionalContentTypes) :
            base(contentType, additionalContentTypes)
        {
        }
    }
}
