#nullable disable
using System;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FormParamAttribute : NamedParamAttribute
    {
        public FormParamAttribute(string name = null) : base(name)
        {
        }
    }
}