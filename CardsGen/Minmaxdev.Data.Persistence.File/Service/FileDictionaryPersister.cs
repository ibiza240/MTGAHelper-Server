using Minmaxdev.Common;
using Minmaxdev.Data.Persistence.Common.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Minmaxdev.Data.Persistence.File.Service;

namespace Minmaxdev.Data.Persistence.File.Service
{
    public class FileDictionaryPersister<TModel> : DocumentDictionaryPersisterBase<TModel>, IDocumentDictionaryPersister<TModel> where TModel : IId
    {
        private readonly FileManipulator<TModel> fileManipulator;

        private string GetFilePath(Guid id) => Path.Combine(documentKey, $"{id}.json");

        public FileDictionaryPersister(
            FilePersisterConfiguration<TModel> configuration
            )
            : base(configuration)
        {
            this.fileManipulator = configuration.FileManipulator;
        }

        public override async Task<bool> Exists(Guid key) => System.IO.File.Exists(GetFilePath(key));

        //protected string FullKey(string subKey) => $"{expirationConfig.CacheKey}_{subKey}";

        public override async Task<TModel> Load(Guid id) => await fileManipulator.Load(GetFilePath(id), configuration.LogDocumentNotFound);

        public override async Task<ICollection<Guid>> LoadAllIds() =>
            Directory.GetFiles(documentKey)
                .Select(i => Path.GetFileNameWithoutExtension(i))
                .Select(i => new Guid(i))
                .ToArray();

        public override async Task Save(TModel data)
        {
            await fileManipulator.Save(GetFilePath(data.Id), data);
        }
    }
}