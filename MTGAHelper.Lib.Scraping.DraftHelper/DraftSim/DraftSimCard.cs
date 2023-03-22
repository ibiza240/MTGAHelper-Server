using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib.Scraping.DraftHelper.DraftSim
{
    public class DraftSimCard
    {
        public string name { get; set; }
        public string castingcost1 { get; set; }
        public string castingcost2 { get; set; }
        public string type { get; set; }
        public string rarity { get; set; }
        public string myrating { get; set; }
        public string image { get; set; }
        public string cmc { get; set; }
        public float[] colors { get; set; }
        public string creaturesort { get; set; }
        public int colorsort { get; set; }
    }
}
