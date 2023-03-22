//using AutoMapper;
//using MTGAHelper.Entity;
//using MTGAHelper.Lib.IO.Reader.MtgaDataLoc;
//using MTGAHelper.Lib.OutputLogParser.IoC;
//using MTGAHelper.Lib.Logging;
//using Newtonsoft.Json;
//using Serilog;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using MTGAHelper.Lib.IoC;

//namespace MTGAHelper.Lib.IO.Reader.MtgaDataCards
//{
//    public class ReaderMtgaDataCards
//    {
//        ReaderMtgaDataLoc readerDataLoc;
//        IMapper mapper;
//        Dictionary<int, string> texts;

//        public ReaderMtgaDataCards(ReaderMtgaDataLoc readerDataLoc)
//        {
//            mapper = new MapperConfiguration(cfg => cfg.AddProfile(new MapperProfileLib(null))).CreateMapper();
//            this.readerDataLoc = readerDataLoc;
//        }

//        public ICollection<Card> ReadFile(string folder, string fileCards, string fileLoc)
//        {
//            var p = Path.Combine(folder, fileCards);
//            LogExt.LogReadFile(p);
//            var content = File.ReadAllText(p);
//            texts = readerDataLoc.ReadFile(Path.Combine(folder, fileLoc));

//            return Read(content);
//        }

//        public ICollection<Card> Read(string json)
//        {
//            var full = JsonConvert.DeserializeObject<ICollection<MtgaDataCardsRootObject>>(json);
//            IEnumerable<MtgaDataCardsRootObject> data = full
//                //.Where(i => i.isToken || i.isCollectible || i.isCraftable)
//                .Where(i => i.set != "ArenaSUP")
//                ;

//            var result = mapper.Map<ICollection<Card>>(data);

//            foreach (var r in result)
//                r.name = texts[r.titleId];

//            return result;
//        }
//    }
//}