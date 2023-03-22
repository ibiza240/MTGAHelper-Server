using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Lib;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Linq;
using MTGAHelper.Entity;
using MTGAHelper.Lib.UserHistory;
using MTGAHelper.Lib.InventoryUpdatedConverters;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsEconomy : TestsBase
    {
        string folderEconomy = Path.Join(folderData, "Economy");

        [TestMethod]
        public void Test_BoosterOpened()
        {
            var converter = provider.GetRequiredService<BoosterOpenConverter>();

            var boosterOpen = File.ReadAllText(Path.Join(folderEconomy, "InventoryUpdate_BoosterOpen_NewCard0_Wc0_Track0.txt"));
            var raw = JsonConvert.DeserializeObject<InventoryUpdatedRaw>(boosterOpen);
            var res = converter.Convert(raw);
            Assert.IsTrue(res.Cards.All(i => i.IsNew == false));
            Assert.IsTrue(res.Cards.All(i => i.IsWildcard == false));
            Assert.AreEqual(20, res.Gems);
            Assert.AreEqual((3 + 3 + 1 + 1 + 1 + 1 + 1) / 10f, res.VaultProgress);
            Assert.AreEqual(RarityEnum.Unknown, res.WildcardFromTrack);

            boosterOpen = File.ReadAllText(Path.Join(folderEconomy, "InventoryUpdate_BoosterOpen_NewCard1R1U_Wc1U_Track1U.txt"));
            raw = JsonConvert.DeserializeObject<InventoryUpdatedRaw>(boosterOpen);
            res = converter.Convert(raw);
            Assert.AreEqual(2, res.Cards.Count(i => i.IsNew));
            Assert.AreEqual(1, res.Cards.Count(i => i.IsNew && i.Rarity == RarityEnum.Rare));
            Assert.AreEqual(1, res.Cards.Count(i => i.IsNew && i.Rarity == RarityEnum.Uncommon));
            Assert.IsTrue(res.Cards.Single(i => i.IsWildcard).Rarity == RarityEnum.Uncommon);
            Assert.AreEqual(0, res.Gems);
            Assert.AreEqual((1 + 1 + 1 + 1 + 1) / 10f, res.VaultProgress);
            Assert.AreEqual(RarityEnum.Uncommon, res.WildcardFromTrack);

            boosterOpen = File.ReadAllText(Path.Join(folderEconomy, "InventoryUpdate_BoosterOpen_NewCard1R_Wc1C_Track0.txt"));
            raw = JsonConvert.DeserializeObject<InventoryUpdatedRaw>(boosterOpen);
            res = converter.Convert(raw);
            Assert.AreEqual(1, res.Cards.Count(i => i.IsNew));
            Assert.AreEqual(1, res.Cards.Count(i => i.IsNew && i.Rarity == RarityEnum.Rare));
            Assert.IsTrue(res.Cards.Single(i => i.IsWildcard).Rarity == RarityEnum.Common);
            Assert.AreEqual(0, res.Gems);
            Assert.AreEqual((3 + 3 + 1 + 1 + 1 + 1) / 10f, res.VaultProgress);
            Assert.AreEqual(RarityEnum.Unknown, res.WildcardFromTrack);
        }

        [TestMethod]
        public void Test_UserHistoryBuilder_Boosters()
        {
            var builder = provider.GetRequiredService<UserHistorySummaryBuilder>();

            var inventoryUpdates = JsonConvert.DeserializeObject<InventoryUpdatedRaw>(File.ReadAllText(Path.Join(folderEconomy, "InventoryUpdate_BoosterOpen_NewCard0_Wc0_Track0.txt")));

            var res = builder.Build(new[] { inventoryUpdates }, null);
            Assert.AreEqual(-1, res.BoostersChange["ELD"]);
            Assert.AreEqual(0, res.NewCardsCount);
            Assert.AreEqual(0, res.WildcardsChange.Sum(i => i.Value));
            Assert.AreEqual(20, res.GemsChange);
            Assert.AreEqual(1.1f, res.VaultProgressChange);
        }

        [TestMethod]
        public void Test_UserHistoryBuilder_CardsCrafted()
        {
            var builder = provider.GetRequiredService<UserHistorySummaryBuilder>();

            var inventoryUpdates = JsonConvert.DeserializeObject<InventoryUpdatedRaw>(File.ReadAllText(Path.Join(folderEconomy, "InventoryUpdate_CardsCrafted.txt")));

            var res = builder.Build(new[] { inventoryUpdates }, null);
            Assert.AreEqual(3, res.NewCardsCount);
            Assert.AreEqual(-2, res.WildcardsChange[RarityEnum.Uncommon]);
            Assert.AreEqual(-1, res.WildcardsChange[RarityEnum.Rare]);
        }

        [TestMethod]
        public void Test_UserHistoryBuilder_CardsCrafted_Plus_BoosterOpened()
        {
            var builder = provider.GetRequiredService<UserHistorySummaryBuilder>();

            var cardsCrafted = JsonConvert.DeserializeObject<InventoryUpdatedRaw>(File.ReadAllText(Path.Join(folderEconomy, "InventoryUpdate_CardsCrafted.txt")));
            var boosterOpened = JsonConvert.DeserializeObject<InventoryUpdatedRaw>(File.ReadAllText(Path.Join(folderEconomy, "InventoryUpdate_BoosterOpen_NewCard1R1U_Wc1U_Track1U.txt")));

            var res = builder.Build(new[] { cardsCrafted, boosterOpened }, null);
            Assert.AreEqual(5, res.NewCardsCount);
            Assert.AreEqual(0, res.WildcardsChange[RarityEnum.Uncommon]);
            Assert.AreEqual(-1, res.WildcardsChange[RarityEnum.Rare]);
            Assert.AreEqual(0, res.GemsChange);
            Assert.AreEqual((1 + 1 + 1 + 1 + 1) / 10f, res.VaultProgressChange);
        }

        [TestMethod]
        public void Test_UserHistoryBuilder_PostMatchUpdates_Loss()
        {
            var builder = provider.GetRequiredService<UserHistorySummaryBuilder>();

            var postMatchUpdates = JsonConvert.DeserializeObject<PostMatchUpdateRaw>(File.ReadAllText(Path.Join(folderEconomy, "PostMatchUpdate_NoQuest_Loss.txt")));

            var res = builder.Build(null, new[] { postMatchUpdates });

            Assert.AreEqual(0, res.GoldChange);
            Assert.AreEqual(0, res.XpChange);
            Assert.AreEqual(0, res.NewCardsCount);
        }

        [TestMethod]
        public void Test_UserHistoryBuilder_PostMatchUpdates_Quest500_DailyWin1_NoWeekly_BattlePassLvl57()
        {
            var builder = provider.GetRequiredService<UserHistorySummaryBuilder>();

            var postMatchUpdates = JsonConvert.DeserializeObject<PostMatchUpdateRaw>(File.ReadAllText(Path.Join(folderEconomy, "PostMatchUpdate_Quest500_DailyWin1_NoWeekly_BattlePassLvl57.txt")));

            var res = builder.Build(null, new[] { postMatchUpdates });

            Assert.AreEqual(500 + 500 + 250, res.GoldChange);
            Assert.AreEqual(525, res.XpChange);
            Assert.AreEqual(0, res.NewCardsCount);
            Assert.AreEqual(1, res.BoostersChange.Count());
            Assert.AreEqual("ELD", res.BoostersChange.Single().Key);
        }
    }
}
