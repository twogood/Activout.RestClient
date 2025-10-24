using System;

namespace Activout.RestClient
{
    public abstract class TemplateAttribute : Attribute
    {
        protected TemplateAttribute(string? template = null)
        {
            Template = template;
        }

        public string? Template { get; set; }
    }
}