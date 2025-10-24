using System;

namespace Activout.RestClient;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
public class PathAttribute(string template) : TemplateAttribute(template);