using System;

namespace Activout.RestClient;

[AttributeUsage(AttributeTargets.Parameter)]
public class FormParamAttribute(string? name = null) : NamedParamAttribute(name);