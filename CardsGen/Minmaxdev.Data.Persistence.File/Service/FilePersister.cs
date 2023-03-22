using Minmaxdev.Data.Persistence.Common.Service;
using System.Threading.Tasks;
using Minmaxdev.Data.Persistence.File.Service;

namespace Minmaxdev.Data.Persistence.File.Service
{
    public class FilePersister<TModel> : DocumentPersisterBase<TModel>
    {
        private readonly FileManipulator<TModel> fileManipulator;

        public FilePersister(
            FilePersisterConfiguration<TModel> configuration
            )
            : base(configuration)
        {
            this.fileManipulator = configuration.FileManipulator;
        }

        public override async Task<TModel> Load() => await fileManipulator.Load(documentKey, configuration.LogDocumentNotFound);

        public override async Task Save(TModel model) => await fileManipulator.Save(documentKey, model);
    }

    public class FilePersister<TModel, TModelFile> : DocumentPersisterBase<TModel, TModelFile>
    {
        private readonly FileManipulator<TModelFile> fileManipulator;

        public FilePersister(
            FilePersisterConfiguration<TModel, TModelFile> configuration
            )
            : base(configuration)
        {
            this.fileManipulator = configuration.FileManipulator;
        }

        //public override async Task<bool> Exists() => await Task.FromResult(System.IO.File.Exists(documentKey));

        public override async Task<TModel> Load()
        {
            var result = await fileManipulator.Load(documentKey, configuration.LogDocumentNotFound);
            return await ConvertFromFile(result);
        }

        public override async Task Save(TModel model)
        {
            var modelFile = await ConvertToFile(model);
            await SaveRaw(modelFile);
        }

        public async Task SaveRaw(TModelFile model) => await fileManipulator.Save(documentKey, model);
    }
}