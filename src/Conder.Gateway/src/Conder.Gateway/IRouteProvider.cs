using System;
using Microsoft.AspNetCore.Routing;

namespace Conder.Gateway
{
    public interface IRouteProvider
    {
        Action<IEndpointRouteBuilder> Build();
    }
}