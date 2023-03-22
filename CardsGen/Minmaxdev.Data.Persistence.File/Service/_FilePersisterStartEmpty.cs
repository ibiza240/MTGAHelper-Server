//using Minmaxdev.Data.Persistence.File.Service;
//using System.Threading.Tasks;

//namespace Minmaxdev.Data.Persistence.Common.Service
//{
//    public class FilePersisterStartEmpty<TModel> : FilePersister<TModel>
//        where TModel : new()
//    {
//        public FilePersisterStartEmpty(
//            FilePersisterConfiguration<TModel> configuration
//            )
//            : base(configuration)
//        {
//        }

//        public override Task<TModel> Load() => Task.FromResult(new TModel());
//    }

//    public class FilePersisterStartEmpty<TModel, TModelFile> : FilePersister<TModel, TModelFile>
//        where TModel : new()
//    {
//        public FilePersisterStartEmpty(
//            FilePersisterConfiguration<TModel, TModelFile> configuration
//            )
//            : base(configuration)
//        {
//        }

//        public override Task<TModel> Load() => Task.FromResult(new TModel());
//    }
//}