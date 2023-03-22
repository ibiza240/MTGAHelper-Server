using System.Threading.Tasks;

namespace MtgaDecksPro.Tools.Internal.Service
{
    public interface ICommand
    {
        Task Execute();
    }
}