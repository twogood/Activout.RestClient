namespace Activout.RestClient;

public interface IExtendableContext;

public interface IExtendable
{
    IExtendableContext ExtendableContext { get; }
}