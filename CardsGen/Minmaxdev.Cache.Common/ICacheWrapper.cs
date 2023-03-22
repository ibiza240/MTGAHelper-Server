using System.Threading.Tasks;

namespace Minmaxdev.Cache.Common
{
    public interface ICacheWrapper<TModel>
    {
        Task<TModel> Get();

        bool TryGet(out TModel data);

        void Set(TModel data);

        Task ForceExpire();
    }
}