//using MTGAHelper.Lib.Logging;
//using Newtonsoft.Json;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace MTGAHelper.Lib.IO.Reader.MtgaDataLoc
//{
//    public class ReaderMtgaDataLoc
//    {
//        public ReaderMtgaDataLoc()
//        {
//        }

//        public Dictionary<int, string> ReadFile(string filepath)
//        {
//            LogExt.LogReadFile(filepath);
//            var content = File.ReadAllText(filepath);
//            return Read(content);
//        }

//        public Dictionary<int, string> Read(string json)
//        {
//            var full = JsonConvert.DeserializeObject<ICollection<MtgaDataLocRootObject>>(json);
//            return full
//                .First(i => i.isoCode == "en-US")
//                .keys
//                .ToDictionary(i => i.id, i => i.text);
//        }
//    }
//}