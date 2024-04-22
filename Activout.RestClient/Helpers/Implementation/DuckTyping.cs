using ImpromptuInterface;

namespace Activout.RestClient.Helpers.Implementation;

public class DuckTyping : IDuckTyping
{
    public TInterface DuckType<TInterface>(object originalDynamic) where TInterface : class
    {
        return originalDynamic.ActLike<TInterface>();
    }
}