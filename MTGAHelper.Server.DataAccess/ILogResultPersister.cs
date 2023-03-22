using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Entity.MtgaOutputLog;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess
{
    public interface ILogResultPersister
    {
        Task SaveHistoryToDisk(IImmutableUser configUser, OutputLogResult newOutputLogResult);
    }
}