//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Text;
//using AutoMapper;
//using MTGAHelper.Entity;
//using MTGAHelper.Entity.OutputLogParsing;
//using MTGAHelper.Entity.UserHistory;
//using MTGAHelper.Lib.Config;
//using MTGAHelper.Lib.IO.Reader.MtgaOutputLog;
//using MTGAHelper.Lib.IO.Reader.MtgaOutputLog.UnityCrossThreadLogger;
//using MTGAHelper.Server.Data.Files;
//using Newtonsoft.Json;

//namespace MTGAHelper.Server.Data.Files.UserHistory
//{
//    public class GlobalDataLoaderFromFile
//    {
//        FileLoader fileLoader;
//        string folderDataGlobal;

//        public GlobalDataLoaderFromFile(
//            FileLoader fileLoader
//            )
//        {
//            this.fileLoader = fileLoader;
//        }

//        public GlobalDataLoaderFromFile Init(string folderDataGlobal)
//        {
//            this.folderDataGlobal = folderDataGlobal;
//            return this;
//        }

//        public GetSeasonAndRankDetailRaw GetSeasonConfig(int seasonOrdinal)
//        {
//            var filePath = Path.Combine(folderDataGlobal, "seasons", $"GetSeasonAndRankDetailRaw{seasonOrdinal}.json");
//            var fileContent = fileLoader.ReadFileContent(filePath);

//            if (fileContent == null)
//                return null;

//            return JsonConvert.DeserializeObject<GetSeasonAndRankDetailRaw>(fileContent);
//        }
//    }
//}