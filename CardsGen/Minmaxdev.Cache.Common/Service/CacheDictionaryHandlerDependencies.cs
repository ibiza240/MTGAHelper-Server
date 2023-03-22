using AutoMapper;
using Minmaxdev.Common;
using Minmaxdev.Data.Persistence.Common.Service;
using Serilog;

namespace Minmaxdev.Cache.Common.Service
{
    public class CacheDictionaryHandlerDependencies<TModel> where TModel : IId
    {
        public IMapper Mapper { get; }
        public ILogger Logger { get; }
        public ICacheDictionaryWrapper<TModel> CacheWrapper { get; }
        public IDocumentDictionaryPersister<TModel> DocumentPersister { get; }

        public CacheDictionaryHandlerDependencies(
            IMapper mapper,
            ILogger logger,
            ICacheDictionaryWrapper<TModel> cache,
            IDocumentDictionaryPersister<TModel> documentPersister
            )
        {
            Mapper = mapper;
            Logger = logger;
            CacheWrapper = cache;
            DocumentPersister = documentPersister;
        }
    }

    public class CacheDictionaryHandlerDependencies<TModel, TModelFile> : CacheDictionaryHandlerDependencies<TModel>
        where TModel : IId
        where TModelFile : IId
    {
        public CacheDictionaryHandlerDependencies(
            IMapper mapper,
            ILogger logger,
            ICacheDictionaryWrapper<TModel> cache,
            IDocumentDictionaryPersister<TModel, TModelFile> documentPersister
            ) : base(mapper, logger, cache, documentPersister)
        {
        }
    }
}