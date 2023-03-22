using System.Collections.Generic;

namespace MTGAHelper.Lib.Config.News
{
    public class ConfigRootNews
    {
        public ICollection<ConfigModelNews> news { get; set; }
        public ICollection<string> ignored { get; set; }
    }
}