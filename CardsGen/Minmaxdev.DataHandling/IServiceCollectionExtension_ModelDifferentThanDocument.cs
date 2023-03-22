using Microsoft.Extensions.DependencyInjection;
using Minmaxdev.Cache.Common;
using Minmaxdev.Cache.Common.Config;
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
        private static IServiceCollection AddCacheWrapper<TModel, TCacheWrapper>(this IServiceCollection services)
            where TCacheWrapper : class, ICacheWrapper<TModel>
        {
            return services
                .AddSingleton<ICacheWrapper<TModel>, TCacheWrapper>()
#if DEBUG
                .AddSingleton<ICacheExpirationConfig<TModel>>(CacheExpirationConfig<TModel>.DefaultNeverExpires())
#else
                .AddSingleton<ICacheExpirationConfig<TModel>>(CacheExpirationConfig<TModel>.DefaultNeverExpires())
#endif
                ;
        }

        private static IServiceCollection AddCacheDictionaryWrapper<TModel, TCacheWrapper>(this IServiceCollection services)
            where TCacheWrapper : class, ICacheDictionaryWrapper<TModel>
            where TModel : IId
        {
            return services
                .AddSingleton<ICacheDictionaryWrapper<TModel>, TCacheWrapper>()
#if DEBUG
                .AddSingleton<ICacheExpirationConfig<TModel>>(CacheExpirationConfig<TModel>.DefaultNeverExpires())
#else
                .AddSingleton<ICacheExpirationConfig<TModel>>(CacheExpirationConfig<TModel>.DefaultNeverExpires())
#endif
                ;
        }

        public static IServiceCollection AddCacheHandler_CacheMemory_DocumentFile<TModel, TModelFile, TConfigFolder>(this IServiceCollection services, params string[] pathParts)
            where TConfigFolder : IConfigFolder
            where TModel : new()
        {
            return services
                .AddCacheHandlerBase_ModelDifferentThanDocument<TModel, TModelFile, FilePersister<TModel, TModelFile>, MemoryCacheWrapper<TModel>>()
                .AddSingleton<FilePersisterConfiguration<TModel, TModelFile>>()
                .AddSingleton<IDocumentPersister<TModel, TModelFile>>(provider =>
                    provider.GetService<FilePersister<TModel, TModelFile>>().WithPathPartsAndBasePath(provider.GetService<TConfigFolder>().Folder, pathParts) as FilePersister<TModel, TModelFile>);
        }

        private static IServiceCollection AddCacheHandlerBase_ModelDifferentThanDocument<TModel, TModelFile, TDocumentPersister, TCacheWrapper>(this IServiceCollection services)
            where TModel : new()
            where TDocumentPersister : class, IDocumentPersister<TModel, TModelFile>
            where TCacheWrapper : class, ICacheWrapper<TModel>
        {
            return services
                .AddCacheWrapper<TModel, TCacheWrapper>()
                .AddSingleton<IDocumentPersister<TModel, TModelFile>, TDocumentPersister>()
                .AddSingleton<DocumentPersisterConfiguration<TModel>>()
                .AddSingleton<ICacheHandler<TModel>>(provider => provider.GetService<ICacheHandler<TModel, TModelFile>>())
                .AddSingleton<ICacheHandler<TModel, TModelFile>, CacheHandler<TModel, TModelFile>>()
                .AddSingleton<CacheHandlerDependencies<TModel, TModelFile>>()
                ;
        }
    }
}