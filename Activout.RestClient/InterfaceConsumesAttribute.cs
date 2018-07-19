using System;
using Microsoft.AspNetCore.Mvc;

namespace Activout.RestClient
{
    // Because ConsumesAttribute is not valid on interfaces
    [AttributeUsage(AttributeTargets.Interface)]
    public class InterfaceConsumesAttribute : ConsumesAttribute
    {
        public InterfaceConsumesAttribute(string contentType, params string[] additionalContentTypes) :
            base(contentType, additionalContentTypes)
        {
        }
    }
}