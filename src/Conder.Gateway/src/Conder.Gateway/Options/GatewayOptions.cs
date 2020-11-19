using System.Collections.Generic;
using Conder.Gateway.Configuration;

namespace Conder.Gateway.Options
{
    public class GatewayOptions : IOptions
    {
        public IDictionary<string, Module> Modules { get; set; }
        
    }
}