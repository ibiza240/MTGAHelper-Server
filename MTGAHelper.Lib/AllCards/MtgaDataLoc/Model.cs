using System.Collections.Generic;

namespace MTGAHelper.Lib.AllCards.MtgaDataLoc
{
    public class Key
    {
        public int id { get; set; }
        public string text { get; set; }
    }

    public class MtgaDataLocRootObject
    {
        public string langkey { get; set; }
        public string isoCode { get; set; }
        public List<Key> keys { get; set; }
    }
}