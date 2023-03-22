using AutoMapper;
using Serilog;

namespace Minmaxdev.Data.Persistence.Common.Service
{
    public class DocumentPersisterDependencies
    {
        public DocumentPersisterDependencies(
            IMapper mapper,
            ILogger logger
            )
        {
            Mapper = mapper;
            Logger = logger;
        }

        public IMapper Mapper { get; }
        public ILogger Logger { get; }
    }
}