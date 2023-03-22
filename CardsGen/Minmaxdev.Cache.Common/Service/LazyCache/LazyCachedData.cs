using System.Threading.Tasks;

namespace Minmaxdev.Cache.Common.Service.LazyCache
{
    public class LazyCachedData<TData, TLoader, TModel> where TLoader : ILazyCachedDataLoader<TData, TModel>
    {
        public LazyCachedData(TLoader dataLoader)
        {
            this.dataLoader = dataLoader;
        }

        private bool isLoaded;
        private TData Data;
        private readonly TLoader dataLoader;

        public async Task<TData> GetDataAsync(TModel model)
        {
            if (isLoaded == false)
            {
                Data = await dataLoader.LoadData(model);
                isLoaded = true;
            }

            return Data;
        }
    }
}