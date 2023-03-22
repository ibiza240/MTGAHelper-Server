//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using AutoMapper;
//using MTGAHelper.Entity;
//using MTGAHelper.Entity.Config.App;
//using MTGAHelper.Lib.IoC;
//using MTGAHelper.Lib.Logging;
//using Newtonsoft.Json;

//namespace MTGAHelper.Lib.IO.Reader
//{
//    public class AllCardsLoader : ICacheLoader<IReadOnlyDictionary<int, Card>>
//    {
//        readonly string filePathAllCardsCached;

//        System.Diagnostics.Stopwatch watch;
//        readonly Dictionary<string, float> loadingTimes = new Dictionary<string, float>();

//        readonly IMapper mapper;

//        public AllCardsLoader(
//            IDataPath configApp
//            )
//        {
//            var folderData = configApp.FolderData;
//            if (string.IsNullOrEmpty(folderData?.Trim()))
//                folderData = new Util().AppFolder;
//            else if (Path.IsPathRooted(folderData) == false)
//                folderData = Path.Combine(new Util().AppFolder, folderData);

//            filePathAllCardsCached = Path.Combine(folderData, "AllCardsCached2.json");

//            mapper = new MapperConfiguration(cfg => cfg.AddProfile(new MapperProfileOldCardFormat())).CreateMapper();
//        }

//        public IReadOnlyDictionary<int, Card> LoadData()
//        {
//            ICollection<Card2> cards2;

//            watch = System.Diagnostics.Stopwatch.StartNew();
//            var file = Path.GetFullPath(filePathAllCardsCached);
//            if (File.Exists(filePathAllCardsCached))
//            {
//                // Load from file
//                LogExt.LogReadFile(filePathAllCardsCached);
//                cards2 = JsonConvert.DeserializeObject<ICollection<Card2>>(File.ReadAllText(filePathAllCardsCached));
//            }
//            else
//            {
//                //// Build from Scratch (Arena files + Scryfall)
//                //cards2 = allCardsBuilder.Init(configApp.InfoBySet.ToDictionary(i => i.Key, i => i.Value.NbCards)).GetFullCards();

//                //File.WriteAllText(filePathAllCardsCached, JsonConvert.SerializeObject(cards2));

//                throw new NotSupportedException("Cards file needed");
//            }

//            var c = cards2
//                .Select(mapper.Map<Card>)
//                .Append(new Card
//                {
//                    set = "ANA",
//                    grpId = 0,
//                    rarity = "Common",
//                    name = "Unknown",
//                    imageCardUrl = "https://cdn11.bigcommerce.com/s-0kvv9/images/stencil/1280x1280/products/266486/371622/classicmtgsleeves__43072.1532006814.jpg?c=2&imbypass=on"
//                })
//                .ToList();

//            watch.Stop();
//            loadingTimes["CardsDatabase"] = watch.ElapsedMilliseconds / 1000.0f;

//            return c.ToDictionary(i => i.grpId, i => i);
//        }
//    }
//}