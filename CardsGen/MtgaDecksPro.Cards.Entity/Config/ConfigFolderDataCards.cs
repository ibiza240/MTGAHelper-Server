using Minmaxdev.Cache.Common.Model;

namespace MtgaDecksPro.Cards.Entity.Config
{
    public record ConfigFolderDataCards : IConfigFolderDataCards, IConfigFolder
    {
        public string FolderDataCards { get; set; }
        public string Folder => FolderDataCards;
    }
}