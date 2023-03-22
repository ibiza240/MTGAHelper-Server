using System.Threading.Tasks;

namespace Minmaxdev.Cache.Common.Service.LazyCache
{
    public interface ILazyCachedDataLoader<T, TModel>
    {
        Task<T> LoadData(TModel model);
    }
}