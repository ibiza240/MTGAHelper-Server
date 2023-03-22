using MTGAHelper.Server.Data.Files;
using MTGAHelper.Server.Data.Files.UserHistory;
using SimpleInjector;

namespace MTGAHelper.Server.Data.IoC
{
    public static class IServiceCollectionExtension
    {
        public static Container RegisterServicesServerData(this Container container)
        {
            container.RegisterSingleton<FileLoader>();
            container.RegisterSingleton<UserHistoryLoaderFromFile>();
            container.RegisterSingleton<UserHistoryDatesAvailableFromFile>();
            return container;
        }
    }
}
