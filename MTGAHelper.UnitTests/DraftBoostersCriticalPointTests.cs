using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Lib.DraftBoostersCriticalPoint;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class RareDraftingCalculatorTests
    {
        [TestMethod]
        public void Calculate_RaresFromScratch()
        {
            var playerData = new DraftBoostersCriticalPointPlayerInput();
            var assum = new DraftBoostersCriticalPointAssumptions
            {
                NbRewardPacksPerDraft = 1.38f,
                NbRaresPerDraft = 3.5f,
                NbMythicsPerDraft = 0.5f
            };

            var result = new DraftBoostersCriticalPointCalculator().Calculate(playerData, assum);

            // assertion numbers taken from the article at https://www.mtggoldfish.com/articles/collecting-mtg-arena-part-1-of-2
            Assert.AreEqual(47, (int)Math.Ceiling(result.ExpectedNbDraftsToPlaysetRares));
        }

        [TestMethod]
        public void Calculate_MythicsFromScratch()
        {
            var playerData = new DraftBoostersCriticalPointPlayerInput();
            var assum = new DraftBoostersCriticalPointAssumptions
            {
                NbRewardPacksPerDraft = 1.38f,
                NbRaresPerDraft = 3.5f,
                NbMythicsPerDraft = 0.5f
            };

            var result = new DraftBoostersCriticalPointCalculator().Calculate(playerData, assum);

            // assertion numbers taken from the article at https://www.mtggoldfish.com/articles/collecting-mtg-arena-part-1-of-2
            Assert.AreEqual(92, (int)Math.Ceiling(result.ExpectedNbDraftsToPlaysetMythics));
        }

        [TestMethod]
        public void Calculate_RaresFromCurrent()
        {
            var playerData = new DraftBoostersCriticalPointPlayerInput
            {
                NbPacks = 57,
                NbRares = 98,
                NbMythics = 17
            };
            var assum = new DraftBoostersCriticalPointAssumptions
            {
                NbRewardPacksPerDraft = 1.3f,
                NbRaresPerDraft = 3,
                NbMythicsPerDraft = 0.5f
            };

            var result = new DraftBoostersCriticalPointCalculator().Calculate(playerData, assum);

            // assertion numbers taken from Excel tracking; multiplied to prevent floating point precision shenanigans
            Assert.AreEqual(1689, (int)Math.Ceiling(result.ExpectedNbDraftsToPlaysetRares * 100));
        }

        [TestMethod]
        public void Calculate_MythicsFromCurrent()
        {
            var playerData = new DraftBoostersCriticalPointPlayerInput
            {
                NbPacks = 57,
                NbRares = 98,
                NbMythics = 17
            };
            var assum = new DraftBoostersCriticalPointAssumptions
            {
                NbRewardPacksPerDraft = 1.3f,
                NbRaresPerDraft = 3,
                NbMythicsPerDraft = 0.5f
            };

            var result = new DraftBoostersCriticalPointCalculator().Calculate(playerData, assum);

            // assertion numbers taken from Excel tracking; multiplied to prevent floating point precision shenanigans
            Assert.AreEqual(5620, (int)Math.Ceiling(result.ExpectedNbDraftsToPlaysetMythics * 100));
        }
    }
}