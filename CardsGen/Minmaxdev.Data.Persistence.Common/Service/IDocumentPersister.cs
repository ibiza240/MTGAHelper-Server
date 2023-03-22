using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minmaxdev.Data.Persistence.Common.Service
{
    public interface IDocumentPersister<TModel>
    {
        Task<TModel> Load();

        Task Save(TModel data);

        IDocumentPersister<TModel> WithPathPartsAndBasePath(IEnumerable<string> basePath, params string[] pathParts);

        IDocumentPersister<TModel> WithPathPartsAndBasePath(string basePath, IEnumerable<string> pathParts);

        IDocumentPersister<TModel> WithPathParts(params string[] pathParts);
    }

    public interface IDocumentPersister<TModel, TModelFile> : IDocumentPersister<TModel>
    {
        Task<TModel> ConvertFromFile(TModelFile modelFile);

        Task<TModelFile> ConvertToFile(TModel model);

        //new IDocumentPersister<TModel, TModelFile> WithPathPartsAndBasePath(IEnumerable<string> basePath, params string[] pathParts);

        //new IDocumentPersister<TModel, TModelFile> WithPathPartsAndBasePath(string basePath, IEnumerable<string> pathParts);

        //new IDocumentPersister<TModel, TModelFile> WithPathParts(params string[] pathParts);
    }
}