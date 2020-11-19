namespace Conder.Gateway
{
    public interface IOptionsProvider
    {
        T Get<T>(string name = null) where T : class, IOptions, new();
        T GetForExtensions<T>(string name) where T : class, IOptions, new();
    }
}