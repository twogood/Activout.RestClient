namespace Activout.RestClient;

public record Part(
    object? Content,
    string? Name = null,
    string? FileName = null);

public record Part<T>(
    T Content,
    string? Name = null,
    string? FileName = null) : Part(Content, Name, FileName)
{
    public new T Content
    {
        get => (T)base.Content!;
        init => base.Content = value;
    }
}
