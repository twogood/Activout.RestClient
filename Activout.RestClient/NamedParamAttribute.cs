using System;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class NamedParamAttribute : Attribute
    {
        public string Name { get; }

        public NamedParamAttribute(string name)
        {
            Name = name;
        }
    }
}