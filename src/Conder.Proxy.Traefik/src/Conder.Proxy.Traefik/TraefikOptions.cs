using System.Collections.Generic;

namespace Conder.Proxy.Traefik
{
    public class TraefikOptions
    {
        public bool Enabled { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}