using Minmaxdev.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minmaxdev.Cache.Common.Service
{
    public interface ICacheDictionaryHandler<TModel> where TModel : IId
    {
        Task<ICollection<TModel>> GetAll();

        Task<TModel> Get(Guid key);

        Task<bool> ExistsAsync(Guid key);

        Task Set(TModel data, bool saveToStore = true);
    }
}