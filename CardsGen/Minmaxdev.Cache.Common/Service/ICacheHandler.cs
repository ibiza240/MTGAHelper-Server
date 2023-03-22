using System.Threading.Tasks;

namespace Minmaxdev.Cache.Common.Service
{
    public interface ICacheHandler<TModel>
    {
        Task<TModel> Get();

        Task Set(TModel data);

        Task ForceExpire();

        Task SaveCacheToDocument();
    }

    public interface ICacheHandler<TModel, TModelFile> : ICacheHandler<TModel>
    {
    }
}