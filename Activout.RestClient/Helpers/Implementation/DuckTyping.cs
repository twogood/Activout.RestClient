using ImpromptuInterface;

namespace Activout.RestClient.Helpers.Implementation
{
    internal class DuckTyping : IDuckTyping
    {
        public TInterface DuckType<TInterface>(object originalDynamic) where TInterface : class
        {
            return originalDynamic.ActLike<TInterface>();
        }
    }
}