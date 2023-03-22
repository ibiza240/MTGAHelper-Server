using System;
using System.Collections.Generic;
using MTGAHelper.Entity.Config.Decks;

namespace MTGAHelper.Lib.Config
{
    [Serializable]
    public class ConfigModelCalendarItem
    {
        public string Title { get; set; }
        public string DateRange { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string ImageAetherhub { get; set; }
    }

    [Serializable]
    public class ConfigModelCalendarImages
    {
        public Dictionary<string, string> ImageBySet { get; set; }
        public Dictionary<string, string> ImageSetByTitle { get; set; }
    }
}
