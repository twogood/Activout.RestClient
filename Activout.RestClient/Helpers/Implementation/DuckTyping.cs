using Activout.RestClient.Implementation;
using ImpromptuInterface;

namespace Activout.RestClient.Helpers.Implementation
{
    class DuckTyping : IDuckTyping
    {
        public TInterface DuckType<TInterface>(object originalDynamic) where TInterface : class
        {
            return Impromptu.ActLike<TInterface>(originalDynamic);
        }
    }
}
