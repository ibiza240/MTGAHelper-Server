using AutoMapper;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Minmaxdev.Data.Persistence.Common.Service
{
    public abstract class DocumentPersisterBase<TModel> : IDocumentPersister<TModel>
    {
        protected readonly ILogger logger;
        protected readonly IMapper mapper;
        protected readonly DocumentPersisterConfiguration<TModel> configuration;

        protected string documentKey;

        public DocumentPersisterBase(
            DocumentPersisterConfiguration<TModel> configuration
            )
        {
            this.logger = configuration.Dependencies.Logger;
            this.mapper = configuration.Dependencies.Mapper;
            this.configuration = configuration;
        }

        public virtual Task<TModel> Load() => default;

        //public virtual Task<bool> Exists() => throw new NotImplementedException($"Exists method not implemented for {GetType().FullName}.");

        public virtual Task Save(TModel data) => throw new NotImplementedException($"Save method not implemented for {GetType().FullName}.");

        public IDocumentPersister<TModel> WithPathPartsAndBasePath(IEnumerable<string> basePath, params string[] pathParts) => WithPathParts(basePath.Union(pathParts).ToArray());

        public IDocumentPersister<TModel> WithPathPartsAndBasePath(string basePath, IEnumerable<string> pathParts) => WithPathParts(new[] { basePath }.Union(pathParts).ToArray());

        public IDocumentPersister<TModel> WithPathParts(params string[] pathParts)
        {
            documentKey = Path.Combine(pathParts);
            return this;
        }
    }
}