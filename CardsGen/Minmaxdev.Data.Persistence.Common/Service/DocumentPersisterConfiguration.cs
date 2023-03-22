namespace Minmaxdev.Data.Persistence.Common.Service
{
    public record DocumentPersisterConfiguration<TModel>(
        DocumentPersisterDependencies Dependencies,
        bool LogDocumentNotFound = true
        );
}