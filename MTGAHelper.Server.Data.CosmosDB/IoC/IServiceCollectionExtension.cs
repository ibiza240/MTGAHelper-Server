using SimpleInjector;

namespace MTGAHelper.Server.Data.CosmosDB.IoC
{
    public static class IServiceCollectionExtension
    {
        public static Container RegisterServicesCosmosDb(this Container container)
        {
            container.RegisterSingleton<UserDataCosmosManager>();
            return container;
        }
    }
}
