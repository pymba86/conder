using System.Collections.Generic;

namespace Conder.Gateway
{
    public interface IExtensionProvider
    {
        IEnumerable<IEnabledExtension> GetAll();
    }
}