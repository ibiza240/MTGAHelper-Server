//using Minmaxdev.Cache.Common.Service;
//using Minmaxdev.Common;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace MtgaDecksPro.Website.Service
//{
//    public class EntityListDocumentManager<T> where T : IId
//    {
//        protected readonly ICacheHandler<List<T>> cacheHandler;

//        public EntityListDocumentManager(
//            ICacheHandler<List<T>> cacheHandler
//            )
//        {
//            this.cacheHandler = cacheHandler;
//        }

//        public async Task AddEvent(T feedback)
//        {
//            var data = await cacheHandler.Get();
//            data.Add(feedback);
//            await cacheHandler.SaveCacheToDocument();
//        }

//        //public async Task RemoveEvent(Predicate<T> condition)
//        //{
//        //    var cache = await cacheHandler.Load();
//        //    cache.RemoveAll(condition);
//        //    await cacheHandler.Save();
//        //}

//        public async Task UpdateEvent(T feedback)
//        {
//            var cache = await cacheHandler.Get();

//            var idx = cache.FindIndex(ind => ind.Id == feedback.Id);
//            cache[idx] = feedback;

//            await cacheHandler.SaveCacheToDocument();
//        }
//    }
//}