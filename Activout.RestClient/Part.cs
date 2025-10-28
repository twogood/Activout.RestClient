#nullable disable
namespace Activout.RestClient
{
    public class Part
    {
        internal object InternalContent { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
    }

    public class Part<T> : Part
    {
        public T Content
        {
            get => (T)InternalContent;
            set => InternalContent = value;
        }
    }
}