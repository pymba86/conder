using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Conder.WebApi
{
    public interface IEndpointsBuilder
    {
        IEndpointsBuilder Get(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false,
            string roles = null, params string[] policies);

        IEndpointsBuilder Get<T>(string path, Func<T, HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false,
            string roles = null, params string[] policies)
            where T : class;

        IEndpointsBuilder Get<TRequest, TResult>(string path, Func<TRequest, HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] polices)
            where TRequest : class;

        IEndpointsBuilder Post(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false,
            string roles = null, params string[] polices);
        
        IEndpointsBuilder Post<T>(string path, Func<T, HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
            where T : class;

        IEndpointsBuilder Put(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies);

        IEndpointsBuilder Put<T>(string path, Func<T, HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
            where T : class;

        IEndpointsBuilder Delete(string path, Func<HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies);

        IEndpointsBuilder Delete<T>(string path, Func<T, HttpContext, Task> context = null,
            Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string roles = null,
            params string[] policies)
            where T : class;
    }
}