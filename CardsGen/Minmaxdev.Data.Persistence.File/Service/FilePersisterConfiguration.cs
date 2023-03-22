using Minmaxdev.Data.Persistence.File.Service;
using Minmaxdev.Data.Persistence.Common.Service;

namespace Minmaxdev.Data.Persistence.File.Service
{
    public record FilePersisterConfiguration<TModel> : DocumentPersisterConfiguration<TModel>
    {
        public FilePersisterConfiguration(
            DocumentPersisterConfiguration<TModel> original,
            FileManipulator<TModel> fileManipulator
            )
            : base(original)
        {
            FileManipulator = fileManipulator;
        }

        public FileManipulator<TModel> FileManipulator { get; }
    }

    public record FilePersisterConfiguration<TModel, TModelFile> : DocumentPersisterConfiguration<TModel>
    {
        public FilePersisterConfiguration(
            DocumentPersisterConfiguration<TModel> original,
            FileManipulator<TModelFile> fileManipulator
            )
            : base(original)
        {
            FileManipulator = fileManipulator;
        }

        public FileManipulator<TModelFile> FileManipulator { get; }
    }
}
