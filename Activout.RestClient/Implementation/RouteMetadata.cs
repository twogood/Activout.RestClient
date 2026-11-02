using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Activout.RestClient.Implementation;

/*
 * Everything about an interface method that does not depend on the RestClientContext,
 * computed once per process instead of once per client instance.
 */
internal sealed record RouteMetadata(
    Type ReturnType,
    Type ActualReturnType,
    Func<Task<object?>, object>? Converter,
    ParameterInfo[] Parameters,
    object[][] ParameterAttributes,
    HttpMethod HttpMethod,
    string TemplateSuffix,
    MediaType? ContentType,
    Type? ErrorResponseType,
    HeaderAttribute[] Headers,
    int BodyArgumentIndex)
{
    private static readonly ConcurrentDictionary<MethodInfo, RouteMetadata> Cache = new();

    public static RouteMetadata Get(MethodInfo method) => Cache.GetOrAdd(method, Create);

    public bool IsVoidTask => ReturnType == typeof(Task);

    public bool IsGenericTask => ReturnType.BaseType == typeof(Task) && ReturnType.IsGenericType;

    public bool IsHttpMethodWithBody => IsMethodWithBody(HttpMethod);

    private static bool IsMethodWithBody(HttpMethod httpMethod) =>
        httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put || httpMethod == HttpMethod.Patch;

    private static RouteMetadata Create(MethodInfo method)
    {
        var returnType = method.ReturnType;
        var actualReturnType = returnType == typeof(Task) ? typeof(void)
            : returnType.BaseType == typeof(Task) && returnType.IsGenericType ? returnType.GenericTypeArguments[0]
            : returnType;
        var parameters = method.GetParameters();

        var httpMethod = HttpMethod.Get;
        var template = new StringBuilder();
        MediaType? contentType = null;
        Type? errorResponseType = null;
        var headers = new List<HeaderAttribute>();

        foreach (var attribute in method.GetCustomAttributes(true))
            switch (attribute)
            {
                case ContentTypeAttribute contentTypeAttribute:
                    contentType = new MediaType(contentTypeAttribute.ContentType);
                    break;
                case ErrorResponseAttribute errorResponseAttribute:
                    errorResponseType = errorResponseAttribute.Type;
                    break;
                case HeaderAttribute headerAttribute:
                    headers.Add(headerAttribute);
                    break;
                case HttpMethodAttribute httpMethodAttribute:
                    template.Append(httpMethodAttribute.Template);
                    httpMethod = httpMethodAttribute.HttpMethod;
                    break;
                case PathAttribute pathAttribute:
                    template.Append(pathAttribute.Template);
                    break;
            }

        var bodyArgumentIndex = -1;
        if (IsMethodWithBody(httpMethod))
        {
            bodyArgumentIndex = parameters.Length - 1;
            if (parameters.Length > 0 && parameters[bodyArgumentIndex].ParameterType == typeof(CancellationToken))
            {
                bodyArgumentIndex--;
            }

            if (bodyArgumentIndex < 0)
            {
                throw new InvalidOperationException("No body argument found for method: " + method.Name);
            }
        }

        return new RouteMetadata(
            ReturnType: returnType,
            ActualReturnType: actualReturnType,
            Converter: actualReturnType == typeof(void) ? null : TaskConverter.Create(actualReturnType),
            Parameters: parameters,
            ParameterAttributes: Array.ConvertAll(parameters, p => p.GetCustomAttributes(false)),
            HttpMethod: httpMethod,
            TemplateSuffix: template.ToString(),
            ContentType: contentType,
            ErrorResponseType: errorResponseType,
            Headers: headers.ToArray(),
            BodyArgumentIndex: bodyArgumentIndex);
    }
}
