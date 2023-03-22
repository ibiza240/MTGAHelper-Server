using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Lib;
using MTGAHelper.Lib.OutputLogParser;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MTGAHelper.UnitTests
{
    [TestClass, Ignore("take long or require specific directory")]
    public class TestsLoaders : TestsBase
    {
        //    [TestMethod]
        //    public void Test_SameCardWithMultipleSets_CorrectCount()
        //    {
        //        // Arrange
        //        // 2 copies of Air Elemental from different sets
        //        var collectionCards = new Dictionary<int, int> {
        //            { 66051, 1 },    // Air Elemental,XLN
        //            { 68298, 1 },    // Air Elemental,M19
        //        };

        //        // Act
        //        var collection = new ReaderCollection(allCards, mappings).LoadCollection(collectionCards);

        //        // Assert
        //        // Air elementals are grouped and counted as 2 copies, regardless of the set
        //        Assert.AreEqual(1, collection.Count);
        //        Assert.AreEqual(2, collection.Single().Amount);
        //    }

        //[TestMethod]
        //public void Test_Load_OutputLogOld()
        //{
        //    var filePathLog = Path.Combine(root, "output_log_RNA.txt");

        //    var res = provider.GetRequiredService<IReaderMtgaOutputLogOld>().Init(allCards).LoadFile(ConfigManagerUsers.USER_LOCAL, filePathLog);
        //}

        //[TestMethod]
        //public void Test_AllCardsBuilder2()
        //{
        //    var fData = Path.Combine(root, "data");
        //    var b = provider.GetRequiredService<AllCardsBuilder2>().Init(provider.GetService<ConfigModelApp>().InfoBySet.ToDictionary(i => i.Key, i => i.Value.NbCards));
        //    var test = b.GetFullCards();
        //    File.WriteAllText(Path.Combine(fData, "AllCardsCached2.json"), JsonConvert.SerializeObject(test));
        //}

        [TestMethod]
        public void Test_Load_OutputLog()
        {
            var filePathLog = Path.Combine(@"C:\Users\BL\AppData\LocalLow\Wizards Of The Coast\MTGA", "Player.log");
            var reader = provider.GetRequiredService<ReaderMtgaOutputLog>()/*.Init(Path.Combine(root, "data"))*/;

            //var res = reader.LoadFile(ConfigManagerUsers.USER_LOCAL, filePathLog);
            var res = reader.LoadFileContent(userTest, new FileStream(filePathLog, FileMode.Open)).Result;

            var sessionContainer = provider.GetRequiredService<ISessionContainer>();
            sessionContainer.SaveToDatabase(userTest, "", res.result2);
        }

        [TestMethod]
        public async Task Test_Load_OutputLog_All()
        {
            var sessionContainer = provider.GetRequiredService<ISessionContainer>();
            //sessionContainer.ReloadMasterData(false);
            await sessionContainer.RegisterUser(userTest);

            var zipFiles = Directory.GetFiles(root, "*output_log*.zip");

            foreach (var f in zipFiles)
            {
                var reader = provider.GetRequiredService<ZipDeflator>();
                var res = reader.UnzipAndGetCollection(userTest, new FileStream(f, FileMode.Open)).Result;
                if (res.errorId == null)
                {
                    await sessionContainer.SetUserCollection(userTest, res.result);
                }
                else
                {
                    Debugger.Break();
                }
            }
        }

        [TestMethod]
        public void TestBinaryPayload()
        {
            var payload = "CDYYzgQgjgfKAuMBCuABCrQBhZgEhZgEmp8Emp8Emp8Emp8EspcEt4cEt4cEsaMEsaMEsaMEsaMEsaMEsaMEsaMEyYoEyYoEyYoEyYoE8I4E8I4Eu4YEu4YEu4YEu4YE/pgE/pgE/pgE/pgE1YoEqaMEqaMEqaMEl6MEl6MEgJQEr4UEr4UEr4UE4YYE4YYE4YYE4YYEkJAErJ8EkaME+pgE+pgE+ZgE+ZgEnJIEnJIEnJIEh6IEh6IE36IE36IE36IEyp4EEifmnwTmnwTmnwTmnwTCjQTCjQTWmATWmATwjgTwjgTgogSdnwTOnAQ=";
            var bytes = Convert.FromBase64String(payload);
            var str = Encoding.ASCII.GetString(bytes);
        }

        //[TestMethod]
        //public void Test_DraftRatingsLoader()
        //{
        //    void GetInfo(Dictionary<string, DraftRatings> ratings, string set)
        //    {
        //        var eldLsv = ratings["ChannelFireball (LSV)"].RatingsBySet[set].Ratings;
        //        var eldDraftSim = ratings["DraftSim"].RatingsBySet[set].Ratings;
        //        var diff1 = eldLsv.Where(i => eldDraftSim.Any(x => x.CardName == i.CardName) == false).ToArray();
        //        var diff2 = eldDraftSim.Where(i => eldLsv.Any(x => x.CardName == i.CardName) == false).ToArray();
        //    }

        //    var cache = provider.GetRequiredService<CacheSingleton<IReadOnlyDictionary<string, DraftRatings>>>();
        //    var ratings = cache.Get();
        //    GetInfo(ratings, "ELD");
        //    GetInfo(ratings, "M20");
        //    GetInfo(ratings, "WAR");
        //    GetInfo(ratings, "RNA");
        //    GetInfo(ratings, "GRN");

        //    var test = provider.GetRequiredService<CacheSingleton<IReadOnlyDictionary<int, Card>>>().Get();
        //}
    }
}