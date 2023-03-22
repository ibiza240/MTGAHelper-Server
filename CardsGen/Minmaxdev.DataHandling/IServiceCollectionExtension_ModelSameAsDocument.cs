using Microsoft.Extensions.DependencyInjection;
using Minmaxdev.Cache.Common;
using Minmaxdev.Cache.Common.Model;
using Minmaxdev.Cache.Common.Service;
using Minmaxdev.Cache.Memory.Service;
using Minmaxdev.Common;
using Minmaxdev.Data.Persistence.Common.Service;
using Minmaxdev.Data.Persistence.File.Service;

namespace Minmaxdev.DataHandling
{
    public static partial class IServiceCollectionExtension
    {
        public static IServiceCollection AddCacheHandler_CacheMemory_DocumentFile<TModel, TConfigFolder>(this IServiceCollection services, params string[] pathParts)
            where TModel : new()
            where TConfigFolder : IConfigFolder
        {
            return services
                .AddSingleton<IDocumentPersister<TModel>>(provider =>
                    provider.GetRequiredService<FilePersister<TModel>>().WithPathPartsAndBasePath(provider.GetRequiredService<TConfigFolder>().Folder, pathParts))
                .AddSingleton<DocumentPersisterConfiguration<TModel>>()
                .AddSingleton<FilePersisterConfiguration<TModel>>()
                .AddCacheHandlerBase_ModelSameAsDocumentModel<TModel, CacheHandler<TModel>, MemoryCacheWrapper<TModel>>();
        }

        public static IServiceCollection AddCacheDictionaryHandler_CacheMemory_DocumentFile<TModel, TConfigFolder>(this IServiceCollection services, params string[] pathParts)
            where TModel : IId, new()
            where TConfigFolder : IConfigFolder
        {
            return services
                .AddSingleton<IDocumentDictionaryPersister<TModel>>(provider =>
                    provider.GetService<FileDictionaryPersister<TModel>>().WithPathPartsAndBasePath(provider.GetService<TConfigFolder>().Folder, pathParts))
                .AddSingleton<FileDictionaryPersister<TModel>>()
                .AddSingleton<DocumentPersisterConfiguration<TModel>>()
                .AddSingleton<FilePersisterConfiguration<TModel>>()
                .AddCacheDictionaryHandlerBase_ModelSameAsDocumentModel<TModel, CacheDictionaryHandler<TModel>, MemoryCacheDictionaryWrapper<TModel>>();
        }

        public static IServiceCollection AddCacheHandler_CacheMemory_Derived<TModel, TMemoryPersister>(this IServiceCollection services)
            where TModel : new()
            where TMemoryPersister : class, IDocumentPersister<TModel>
        {
            return services.AddCacheHandlerBase_ModelSameAsDocumentModel<TModel, TMemoryPersister, CacheHandler<TModel>, MemoryCacheWrapper<TModel>>();
        }

        public static IServiceCollection AddCacheHandler_CacheMemory_CustomDocumentPersister<TModel, TDocumentPersister>(this IServiceCollection services)
            where TModel : new()
            where TDocumentPersister : class, IDocumentPersister<TModel>
        {
            return services
                .AddCacheHandlerBase_ModelSameAsDocumentModel<TModel, TDocumentPersister, CacheHandler<TModel>, MemoryCacheWrapper<TModel>>();
        }

        public static IServiceCollection AddCacheHandler_CacheMemory_CustomDocumentPersister_CustomCacheHandler<TModel, TDocumentPersister, TCacheHandler>(this IServiceCollection services, bool logMissingDocument = true)
            where TModel : new()
            where TDocumentPersister : class, IDocumentPersister<TModel>
            where TCacheHandler : class, ICacheHandler<TModel>
        {
            return services
                .AddSingleton<TCacheHandler>()
                .AddCacheHandlerBase_ModelSameAsDocumentModel<TModel, TDocumentPersister, TCacheHandler, MemoryCacheWrapper<TModel>>(logMissingDocument);
        }

        private static IServiceCollection AddCacheHandlerBase_ModelSameAsDocumentModel<TModel, TCacheHandler, TCacheWrapper>(this IServiceCollection services)
            where TCacheWrapper : class, ICacheWrapper<TModel>
            where TCacheHandler : class, ICacheHandler<TModel>
        {
            return services
                .AddCacheWrapper<TModel, TCacheWrapper>()
                .AddSingleton<ICacheHandler<TModel>, TCacheHandler>()
                .AddSingleton<CacheHandlerDependencies<TModel>>();
        }

        private static IServiceCollection AddCacheDictionaryHandlerBase_ModelSameAsDocumentModel<TModel, TCacheHandler, TCacheWrapper>(this IServiceCollection services)
            where TModel : IId
            where TCacheWrapper : class, ICacheDictionaryWrapper<TModel>
            where TCacheHandler : class, ICacheDictionaryHandler<TModel>
        {
            return services
                .AddCacheDictionaryWrapper<TModel, TCacheWrapper>()
                .AddSingleton<ICacheDictionaryHandler<TModel>, TCacheHandler>()
                .AddSingleton<CacheDictionaryHandlerDependencies<TModel>>();
        }

        private static IServiceCollection AddCacheHandlerBase_ModelSameAsDocumentModel<TModel, TDocumentPersister, TCacheHandler, TCacheWrapper>(this IServiceCollection services, bool logMissingDocument = true)
            where TDocumentPersister : class, IDocumentPersister<TModel>
            where TCacheWrapper : class, ICacheWrapper<TModel>
            where TCacheHandler : class, ICacheHandler<TModel>
        {
            return services
                .AddCacheHandlerBase_ModelSameAsDocumentModel<TModel, TCacheHandler, TCacheWrapper>()
                .AddSingleton<IDocumentPersister<TModel>, TDocumentPersister>()
                .AddSingleton<DocumentPersisterConfiguration<TModel>>()
                .AddSingleton(provider => new FilePersisterConfiguration<TModel>(
                    new DocumentPersisterConfiguration<TModel>(provider.GetRequiredService<DocumentPersisterDependencies>(), logMissingDocument),
                    provider.GetRequiredService<FileManipulator<TModel>>()
                    )
                );
        }
    }
}