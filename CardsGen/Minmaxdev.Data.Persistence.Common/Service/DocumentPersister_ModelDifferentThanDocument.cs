using System.Threading.Tasks;

namespace Minmaxdev.Data.Persistence.Common.Service
{
    public abstract class DocumentPersisterBase<TModel, TModelFile> : DocumentPersisterBase<TModel>, IDocumentPersister<TModel, TModelFile>
    {
        public DocumentPersisterBase(DocumentPersisterConfiguration<TModel> configuration)
            : base(configuration)
        {
        }

        public virtual Task<TModel> ConvertFromFile(TModelFile modelFile) => Task.FromResult(mapper.Map<TModel>(modelFile));

        public virtual Task<TModelFile> ConvertToFile(TModel model) => Task.FromResult(mapper.Map<TModelFile>(model));
    }
}