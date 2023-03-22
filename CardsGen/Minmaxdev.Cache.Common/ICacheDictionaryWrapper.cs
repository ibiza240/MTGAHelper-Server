using Minmaxdev.Common;
using System;

namespace Minmaxdev.Cache.Common
{
    public interface ICacheDictionaryWrapper<TModel> where TModel : IId
    {
        TModel Get(Guid id);

        bool TryGet(Guid id, out TModel data);

        void Set(TModel data);
    }
}