namespace Activout.RestClient
{
    public class AcceptAttribute : HeaderAttribute
    {
        public AcceptAttribute(string value) : base("Accept", value)
        {
        }
    }
}