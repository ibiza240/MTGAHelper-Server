using AutoMapper;
using Minmaxdev.Data.Persistence.Common.Service;
using Serilog;

namespace Minmaxdev.Cache.Common.Service
{
    public class CacheHandlerDependencies<TModel>
    {
        public IMapper Mapper { get; }
        public ILogger Logger { get; }
        public ICacheWrapper<TModel> CacheWrapper { get; }
        public IDocumentPersister<TModel> DocumentPersister { get; }

        public CacheHandlerDependencies(
            IMapper mapper,
            ILogger logger,
            ICacheWrapper<TModel> cache,
            IDocumentPersister<TModel> documentPersister
            )
        {
            Mapper = mapper;
            Logger = logger;
            CacheWrapper = cache;
            DocumentPersister = documentPersister;
        }
    }

    public class CacheHandlerDependencies<TModel, TModelFile> : CacheHandlerDependencies<TModel>
    {
        public CacheHandlerDependencies(
            IMapper mapper,
            ILogger logger,
            ICacheWrapper<TModel> cache,
            IDocumentPersister<TModel, TModelFile> documentPersister
            ) : base(mapper, logger, cache, documentPersister)
        {
        }
    }
}