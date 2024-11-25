using System;

namespace Activout.RestClient.Implementation;

internal record ExtendableContextImpl(RestClientContext Context, Type Type) : IExtendableContext;