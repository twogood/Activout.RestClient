using System;
using System.Net.Http;

namespace Activout.RestClient;

[AttributeUsage(AttributeTargets.Method)]
public abstract class HttpMethodAttribute(HttpMethod httpMethod, string? template) : TemplateAttribute(template)
{
    public HttpMethod HttpMethod { get; } = httpMethod;
}

public class DeleteAttribute(string? template = null) : HttpMethodAttribute(HttpMethod.Delete, template);

public class GetAttribute(string? template = null) : HttpMethodAttribute(HttpMethod.Get, template);

public class PostAttribute(string? template = null) : HttpMethodAttribute(HttpMethod.Post, template);

public class PutAttribute(string? template = null) : HttpMethodAttribute(HttpMethod.Put, template);

public class PatchAttribute(string? template = null) : HttpMethodAttribute(HttpMethod.Patch, template);