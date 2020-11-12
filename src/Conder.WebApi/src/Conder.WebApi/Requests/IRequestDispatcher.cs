using System.Threading.Tasks;

namespace Conder.WebApi.Requests
{
    public interface IRequestDispatcher
    {
        Task<TResult> DispatchAsync<TRequest, TResult>(TRequest request) where TRequest : class, IRequest;
    }
}