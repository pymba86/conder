using System.Threading.Tasks;

namespace Conder.Types
{
    public interface IInitializer
    {
        Task InitializeAsync();
    }
}