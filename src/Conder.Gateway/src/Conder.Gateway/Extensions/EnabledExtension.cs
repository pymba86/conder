namespace Conder.Gateway.Extensions
{
    public class EnabledExtension : IEnabledExtension
    {
        public IExtension Extension { get; }
        public IExtensionOptions Options { get; }
        
        public EnabledExtension(IExtension extension, IExtensionOptions options)
        {
            Options = options;
            Extension = extension;
        }
    }
}