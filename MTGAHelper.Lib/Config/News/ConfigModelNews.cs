using System;
using MTGAHelper.Entity.Config.Decks;

namespace MTGAHelper.Lib.Config.News
{
    [Serializable]
    public class ConfigModelNews : IConfigModel
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime DateScraped { get; set; } = DateTime.Now;

        public ConfigModelNews()
        {
        }
    }
}