using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib.Scraping.CalendarScraper
{
    public class CalendarScraperMtgaAssistantMTGAFrontpage
    {
        public CalendarScraperMtgaAssistantMTGAFrontpageModel Model { get; set; }
    }

    public class CalendarScraperMtgaAssistantMTGAFrontpageModel
    {
        public string Calendar { get; set; }
        // Other stuff
    }

    public class CalendarScraperMtgaAssistantItemModel
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }
}
