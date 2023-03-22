using Minmaxdev.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minmaxdev.Data.Persistence.Common.Service
{
    public interface IDocumentDictionaryPersister<TModel> where TModel : IId
    {
        public Task<ICollection<Guid>> LoadAllIds();

        Task<bool> Exists(Guid key);

        Task<TModel> Load(Guid id);

        Task Save(TModel data);
    }

    public interface IDocumentDictionaryPersister<TModel, TModelFile> : IDocumentDictionaryPersister<TModel> where TModel : IId
    {
    }
}