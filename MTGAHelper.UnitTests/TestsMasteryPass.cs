using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Lib.MasteryPass;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsMasteryPass
    {
        static readonly DateTime IKO_END = new DateTime(2020, 6, 26, 18, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void TestConfigDecks()
        {
            var inputs = new MasteryPassCalculatorInputs
            {
                CurrentDateUtc = DateTime.UtcNow,
                CurrentLevel = 42,
                CurrentXp = 200,
                DailyWinsCompleted = 4,
                WeeklyWinsCompleted = 6,
                DailyQuestsAvailable = 1,
            };

            var calc = new MasteryPassCalculator();
            calc.EstimateFinalLevel("ELD", inputs);
        }

        [TestMethod]
        public void TestNoWeeksLeft()
        {
            var dayBeforeEnd = new DateTime(2020, 6, 25, 0, 0, 0, DateTimeKind.Utc);
            var result = MasteryPassCalculator.CalculateNbWeeksLeft(dayBeforeEnd, IKO_END);

            result.Should().Be(0, "currentDate is in last week");
        }

        [TestMethod]
        public void TestTwoWeeksLeft()
        {
            var morningBeforeWeeklyReset = new DateTime(2020, 6, 14, 6, 0, 0, DateTimeKind.Utc);
            var result = MasteryPassCalculator.CalculateNbWeeksLeft(morningBeforeWeeklyReset, IKO_END);

            result.Should().Be(2);
        }

        [TestMethod]
        public void TestOneWeekLeft()
        {
            var justAfterWeeklyReset = new DateTime(2020, 6, 14, 13, 0, 0, DateTimeKind.Utc);
            var result = MasteryPassCalculator.CalculateNbWeeksLeft(justAfterWeeklyReset, IKO_END);

            result.Should().Be(1);
        }
    }
}
