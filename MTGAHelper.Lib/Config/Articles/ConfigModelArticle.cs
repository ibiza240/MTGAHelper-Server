using System;
using MTGAHelper.Entity.Config.Decks;

namespace MTGAHelper.Lib.Config.Articles
{
    [Serializable]
    public class ConfigModelArticle : IConfigModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Text { get; set; }
        public DateTime DatePosted { get; set; }

        public ConfigModelArticle()
        {
        }
    }
}