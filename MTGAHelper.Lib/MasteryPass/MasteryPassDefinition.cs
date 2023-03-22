using System;

namespace MTGAHelper.Lib.MasteryPass
{
    public class MasteryPassDefinition
    {
        public string Name { get; set; }
        public int MaxLevel { get; set; }
        public DateTime DateStartUtc { get; set; }
        public DateTime DateEndUtc { get; set; }
    }
}