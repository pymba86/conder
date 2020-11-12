using System.Threading.Tasks;

namespace Conder.WebApi.Requests
{
    public interface IRequestHandler<in TRequest, TResult> where TRequest : class, IRequest
    {
        Task<TResult> HandleAsync(TRequest request);
    }
}