#nullable disable
namespace Activout.RestClient.Helpers
{
    public interface IDuckTyping
    {
        TInterface DuckType<TInterface>(object originalDynamic) where TInterface : class;
    }
}