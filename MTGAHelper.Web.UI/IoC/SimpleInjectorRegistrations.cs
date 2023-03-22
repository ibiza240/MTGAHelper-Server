using MTGAHelper.Server.DataAccess;
using MTGAHelper.Web.UI.Helpers;
using MTGAHelper.Web.UI.Shared;
using SimpleInjector;

namespace MTGAHelper.Web.UI.IoC
{
    public static class SimpleInjectorRegistrations
    {
        public static Container RegisterServicesApp(this Container container)
        {
            container.RegisterSingleton<MapperConverterLimitedResultsSummary>();
            container.RegisterSingleton<FilesHashManager>();
            container.RegisterSingleton<AccountRepository>();
            container.Collection.Append<AutoMapper.Profile, MapperProfileWebUI>(Lifestyle.Singleton);
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(LoggingQueryHandlerDecorator<,>));

            // Transient
            container.Register<IFileCollectionDeflator, FileCollectionDeflator>();
            container.Register<MessageWriter>();
            container.Register<CollectionExporter>();
            container.Register<EmailSender>();
            container.Register<ExternalUserProcessor>();

            return container;
        }
    }
}
