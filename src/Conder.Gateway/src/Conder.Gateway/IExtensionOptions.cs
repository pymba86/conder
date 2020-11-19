namespace Conder.Gateway
{
    public interface IExtensionOptions
    {
        int? Order { get; set; }
        bool? Enabled { get; set; }
    }
}