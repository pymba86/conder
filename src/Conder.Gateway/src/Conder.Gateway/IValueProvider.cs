using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conder.Gateway
{
    internal interface IValueProvider
    {
        IEnumerable<string> Tokens { get; }
        string Get(string value, HttpRequest request, RouteData data);
    }
}