using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTGAHelper.Lib.Scraping.DraftHelper.ChannelFireball
{
    public class UrlToScrapeModel
    {
        public const string UrlTemplate = "https://www.channelfireball.com/articles/{0}-limited-set-review-{1}/";

        public string UrlPartSet { get; set; }
        public Dictionary<string, string> DictUrlPartColor { get; set; }

        public ICollection<string> DebugListOfUrls => DictUrlPartColor.Select(i => string.Format(UrlTemplate, UrlPartSet, i.Value)).ToArray();

        public UrlToScrapeModel(string urlPart, Dictionary<string, string> dictUrlPartColor)
        {
            UrlPartSet = urlPart;
            DictUrlPartColor = dictUrlPartColor;
        }
    }

}
