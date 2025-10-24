using ImpromptuInterface;

namespace Activout.RestClient.Helpers.Implementation
{
    public class DuckTyping : IDuckTyping
    {
        public static IDuckTyping Instance { get; } = new DuckTyping();

        public TInterface DuckType<TInterface>(object originalDynamic) where TInterface : class
        {
            return originalDynamic.ActLike<TInterface>();
        }
    }
}