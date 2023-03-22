using AutoMapper;
using Minmaxdev.Data.Persistence.Common.Service;
using Serilog;
using System.Threading.Tasks;

namespace Minmaxdev.Cache.Common.Service
{
    public class CacheHandler<TModel, TModelFile> : CacheHandler<TModel>, ICacheHandler<TModel, TModelFile>
        where TModel : new()
    {
        public CacheHandler(
            CacheHandlerDependencies<TModel, TModelFile> dependencies
            )
            : base(dependencies)
        {
        }
    }

    public class CacheHandler<TModel> : ICacheHandler<TModel>
        where TModel : new()
    {
        protected readonly ICacheWrapper<TModel> cacheWrapper;
        protected readonly IDocumentPersister<TModel> documentPersister;
        protected readonly ILogger logger;
        protected readonly IMapper mapper;

        public CacheHandler(
            CacheHandlerDependencies<TModel> dependencies
            )
        {
            this.cacheWrapper = dependencies.CacheWrapper;
            this.documentPersister = dependencies.DocumentPersister;
            this.logger = dependencies.Logger;
            this.mapper = dependencies.Mapper;
        }

        public virtual async Task<TModel> Get()
        {
            if (cacheWrapper.TryGet(out TModel data))
                return data;
            else
            {
                var storedData = await documentPersister.Load();
                cacheWrapper.Set(storedData ?? new TModel());
                return storedData;
            }
        }

        public virtual async Task Set(TModel data)
        {
            cacheWrapper.Set(data);
            await documentPersister.Save(data);
        }

        public async Task ForceExpire() => await cacheWrapper.ForceExpire();

        public virtual async Task SaveCacheToDocument()
        {
            var data = await cacheWrapper.Get();
            await documentPersister.Save(data);
        }
    }
}