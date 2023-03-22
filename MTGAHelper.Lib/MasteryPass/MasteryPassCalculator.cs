using System;

namespace MTGAHelper.Lib.MasteryPass
{
    public class MasteryPassCalculator
    {
        public const int NB_QUESTS = 3;

        private const int XP_PER_DAILY_QUEST = 500;

        private const int NB_WEEKLY_WINS = 15;
        private const int XP_PER_WEEKLY_WIN = 250;

        private const int NB_DAILY_WINS = 10;
        private const int XP_PER_DAILY_WIN = 25;

        private const int XP_PER_LEVEL = 1000;

        public MasteryPassCalculatorInputs inputs { get; private set; }
        public MasteryPassDefinition masteryPass { get; private set; }
        private int totalXp;

        public int FinalLevel => totalXp / XP_PER_LEVEL;
        public int NbDaysLeft => (int)(masteryPass.DateEndUtc - inputs.CurrentDateUtc).TotalDays;
        public int NbWeeksLeft => CalculateNbWeeksLeft(inputs.CurrentDateUtc, masteryPass.DateEndUtc);

        public int ExpectedDailyWins => inputs.ExpectedDailyWins;
        public int ExpectedWeeklyWins => inputs.ExpectedWeeklyWins;

        public int ExpectedDailyWinsLimit => Math.Max(0, Math.Min(NB_DAILY_WINS, inputs.ExpectedDailyWins));
        public int ExpectedWeeklyWinsLimit => Math.Max(0, Math.Min(NB_WEEKLY_WINS, inputs.ExpectedWeeklyWins));

        public int XpWorthDailyQuestsToday => inputs.DailyQuestsAvailable * XP_PER_DAILY_QUEST;
        public int XpWorthDailyQuestsFuture => NbDaysLeft * XP_PER_DAILY_QUEST;

        public int XpWorthDailyWinsToday => Math.Max(0, ExpectedDailyWinsLimit - inputs.DailyWinsCompleted) * XP_PER_DAILY_WIN;
        public int XpWorthDailyWinsFuture => NbDaysLeft * Math.Min(10, ExpectedDailyWinsLimit) * XP_PER_DAILY_WIN;

        public int XpWorthWeeklyWinsToday => Math.Max(0, ExpectedWeeklyWinsLimit - inputs.WeeklyWinsCompleted) * XP_PER_WEEKLY_WIN;
        public int XpWorthWeeklyWinsFuture => NbWeeksLeft * ExpectedWeeklyWinsLimit * XP_PER_WEEKLY_WIN;

        private const int RESET_TIME = 9;

        public static DateTime CalculateLastWeeklyWinResetUtc(DateTime nowUtc)
        {
            if (nowUtc.DayOfWeek == DayOfWeek.Sunday && nowUtc.TimeOfDay.TotalHours < RESET_TIME)
                return nowUtc.AddDays(-7).Date.AddHours(RESET_TIME);
            return nowUtc.AddDays(-(int)nowUtc.DayOfWeek).Date.AddHours(RESET_TIME);
        }

        public static DateTime CalculateNextWeeklyWinResetUtc(DateTime fromUtc)
        {
            return CalculateLastWeeklyWinResetUtc(fromUtc).AddDays(7);
        }

        internal static int CalculateNbWeeksLeft(DateTime currentDateUtc, DateTime masteryPassDateEndUtc)
        {
            var nextWeeklyResetUtc = CalculateNextWeeklyWinResetUtc(currentDateUtc);
            var nbWeeksRemainingAfterThisOne = Math.Ceiling((masteryPassDateEndUtc - nextWeeklyResetUtc).TotalDays / 7);
            return (int)nbWeeksRemainingAfterThisOne;
        }

        public MasteryPassDefinition GetDefinition(string name)
        {
            switch (name)
            {
                case "ELD":
                    return new MasteryPassDefinition
                    {
                        Name = "ELD",
                        MaxLevel = 110,
                        DateStartUtc = SetStartingDates.DictStartingDate["ELD"],
                        DateEndUtc = SetStartingDates.DictStartingDate["THB"],
                    };

                case "THB":
                    return new MasteryPassDefinition
                    {
                        Name = "THB",
                        MaxLevel = 110,
                        DateStartUtc = SetStartingDates.DictStartingDate["THB"],
                        DateEndUtc = SetStartingDates.DictStartingDate["IKO"],
                    };

                case "IKO":
                    return new MasteryPassDefinition
                    {
                        Name = "IKO",
                        MaxLevel = 80,
                        DateStartUtc = SetStartingDates.DictStartingDate["IKO"],
                        DateEndUtc = SetStartingDates.DictStartingDate["M21"],
                    };

                case "M21":
                    return new MasteryPassDefinition
                    {
                        Name = "M21",
                        MaxLevel = 90,
                        DateStartUtc = SetStartingDates.DictStartingDate["M21"],
                        DateEndUtc = SetStartingDates.DictStartingDate["ZNR"],
                    };

                case "ZNR":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 130,
                        DateStartUtc = SetStartingDates.DictStartingDate["ZNR"],
                        DateEndUtc = SetStartingDates.DictStartingDate["KHM"],
                    };

                case "KHM":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 80,
                        DateStartUtc = SetStartingDates.DictStartingDate["KHM"],
                        DateEndUtc = SetStartingDates.DictStartingDate["STX"],
                    };

                case "STX":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 90,
                        DateStartUtc = SetStartingDates.DictStartingDate["STX"],
                        DateEndUtc = SetStartingDates.DictStartingDate["AFR"],
                    };

                case "AFR":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 80,
                        DateStartUtc = SetStartingDates.DictStartingDate["AFR"],
                        DateEndUtc = SetStartingDates.DictStartingDate["MID"],
                    };

                case "MID":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 60,
                        DateStartUtc = SetStartingDates.DictStartingDate["MID"],
                        DateEndUtc = SetStartingDates.DictStartingDate["VOW"],
                    };

                case "VOW":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 90,
                        DateStartUtc = SetStartingDates.DictStartingDate["VOW"],
                        DateEndUtc = SetStartingDates.DictStartingDate["NEO"],
                    };

                case "NEO":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 90,
                        DateStartUtc = SetStartingDates.DictStartingDate["NEO"],
                        DateEndUtc = SetStartingDates.DictStartingDate["SNC"],
                    };

                case "SNC":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 70,
                        DateStartUtc = SetStartingDates.DictStartingDate["SNC"],
                        DateEndUtc = SetStartingDates.DictStartingDate["HBG"],
                    };

                case "HBG":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 70,
                        DateStartUtc = SetStartingDates.DictStartingDate["HBG"],
                        DateEndUtc = SetStartingDates.DictStartingDate["DMU"],
                    };

                case "DMU":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 80,
                        DateStartUtc = SetStartingDates.DictStartingDate["DMU"],
                        DateEndUtc = SetStartingDates.DictStartingDate["BRO"],
                    };

                case "BRO":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 80,
                        DateStartUtc = SetStartingDates.DictStartingDate["BRO"],
                        DateEndUtc = SetStartingDates.DictStartingDate["ONE"],
                    };

                case "ONE":
                    return new MasteryPassDefinition
                    {
                        Name = name,
                        MaxLevel = 80,
                        DateStartUtc = SetStartingDates.DictStartingDate["ONE"],
                        DateEndUtc = SetStartingDates.DictStartingDate["MOM"],
                    };
            }

            throw new NotSupportedException($"The mastery pass '{name}' is not supported");
        }

        public void EstimateFinalLevel(string name, MasteryPassCalculatorInputs inputs)
        {
            // Init
            this.inputs = inputs;
            totalXp = inputs.CurrentLevel * XP_PER_LEVEL + inputs.CurrentXp;
            masteryPass = GetDefinition(name);
            AddDailyQuests();
            AddDailyWins();
            AddWeeklyWins();
        }

        private void AddDailyQuests()
        {
            // Remaining quests
            totalXp += XpWorthDailyQuestsToday;
            // One per day until the end
            totalXp += XpWorthDailyQuestsFuture;
        }

        private void AddDailyWins()
        {
            // Remaining of the day
            totalXp += XpWorthDailyWinsToday;
            // Next days until the end
            totalXp += XpWorthDailyWinsFuture;
        }

        private void AddWeeklyWins()
        {
            // Remaining of the week
            totalXp += XpWorthWeeklyWinsToday;
            // Next weeks until the end
            totalXp += XpWorthWeeklyWinsFuture;
        }
    }
}