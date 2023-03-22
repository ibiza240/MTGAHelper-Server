using System.IO;
using MTGAHelper.Entity.Config.App;

namespace MTGAHelper.Tests.Integration
{
    public class DataFolderRoot : IDataPath
    {
        public string FolderData { get; }
        public DataFolderRoot()
        {
            FolderData = Path.Combine(Directory.GetCurrentDirectory(), "../../../../data");
        }
    }
}
