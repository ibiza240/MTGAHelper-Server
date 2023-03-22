using AutoMapper;
using Minmaxdev.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Minmaxdev.Data.Persistence.Common.Service
{
    public abstract class DocumentDictionaryPersisterBase<TModel> : IDocumentDictionaryPersister<TModel> where TModel : IId
    {
        protected readonly ILogger logger;
        protected readonly IMapper mapper;
        protected readonly DocumentPersisterConfiguration<TModel> configuration;

        protected string documentKey;

        public DocumentDictionaryPersisterBase(
            DocumentPersisterConfiguration<TModel> configuration
            )
        {
            this.logger = configuration.Dependencies.Logger;
            this.mapper = configuration.Dependencies.Mapper;
            this.configuration = configuration;
        }

        public abstract Task<TModel> Load(Guid id);

        public abstract Task<ICollection<Guid>> LoadAllIds();

        public abstract Task<bool> Exists(Guid key);

        public abstract Task Save(TModel data);

        public IDocumentDictionaryPersister<TModel> WithPathPartsAndBasePath(IEnumerable<string> basePath, params string[] pathParts) => WithPathParts(basePath.Union(pathParts).ToArray());

        public IDocumentDictionaryPersister<TModel> WithPathPartsAndBasePath(string basePath, IEnumerable<string> pathParts) => WithPathParts(new[] { basePath }.Union(pathParts).ToArray());

        public IDocumentDictionaryPersister<TModel> WithPathParts(params string[] pathParts)
        {
            documentKey = Path.Combine(pathParts);
            return this;
        }
    }
}