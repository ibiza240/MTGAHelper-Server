using Minmaxdev.Common;
using Minmaxdev.Data.Persistence.Common.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minmaxdev.Cache.Common.Service
{
    public class CacheDictionaryHandler<TModel> : ICacheDictionaryHandler<TModel> where TModel : IId
    {
        private readonly ICacheDictionaryWrapper<TModel> cacheWrapper;
        private readonly IDocumentDictionaryPersister<TModel> documentPersister;

        public CacheDictionaryHandler(
            ICacheDictionaryWrapper<TModel> cacheWrapper,
            IDocumentDictionaryPersister<TModel> documentPersister
            )
        {
            this.cacheWrapper = cacheWrapper;
            this.documentPersister = documentPersister;
        }

        public async Task<bool> ExistsAsync(Guid key)
        {
            if (cacheWrapper.TryGet(key, out TModel data))
                return true;
            else
            {
                return await documentPersister.Exists(key);
            }
        }

        public async Task<TModel> Get(Guid key)
        {
            if (cacheWrapper.TryGet(key, out TModel data))
                return data;
            else
            {
                var storedData = await documentPersister.Load(key);

                if (storedData == null)
                    return default;

                cacheWrapper.Set(storedData);
                return storedData;
            }
        }

        public async Task<ICollection<TModel>> GetAll()
        {
            var all = await documentPersister.LoadAllIds();

            var result = await Task.WhenAll(
                all
                .Select(async id => await Get(id))
                .ToArray()
                );

            return result;
        }

        public async Task Set(TModel data, bool saveToStore = true)
        {
            cacheWrapper.Set(data);

            if (saveToStore)
                await documentPersister.Save(data);
        }
    }
}