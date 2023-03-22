using System;

namespace MTGAHelper.Lib.MasteryPass
{
    public class MasteryPassCalculatorInputs
    {
        public DateTime CurrentDateUtc { get; set; }
        public int CurrentLevel { get; set; }
        public int CurrentXp { get; set; }
        public int WeeklyWinsCompleted { get; set; }
        public int DailyWinsCompleted { get; set; }
        public int DailyQuestsAvailable { get; set; }
        public int ExpectedDailyWins { get; set; }
        public int ExpectedWeeklyWins { get; set; }
    }
}