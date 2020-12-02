using System;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class PartParamAttribute : NamedParamAttribute
    {
        public string FileName { get; }
        public MediaType ContentType { get; }

        public PartParamAttribute(string name = null, string fileName = null, string contentType = null) : base(name)
        {
            FileName = fileName;
            ContentType = MediaType.ValueOf(contentType);
        }
    }
}