using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;

namespace MTGAHelper.Lib.Scraping.DeckSources.Aetherhub
{
    public class DeckScraperAetherhubMetaPaper : DeckScraperAetherhubBase
    {
        public DeckScraperAetherhubMetaPaper(
            IDataPath configPath,
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter,
            IDateDeconstructor dateDeconstructor)
            : base(configPath, writerDeck, converter, dateDeconstructor)
        {
        }
    }
}