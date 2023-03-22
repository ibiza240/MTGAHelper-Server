//using CsvHelper;
//using CsvHelper.Configuration;
//using MTGAHelper.Entity;
//using System.Globalization;
//using System.IO;
//using System.Linq;

//namespace MTGAHelper.Lib.IO.Writer.WriterDeckTypes
//{
//    public interface IWriterDeckAverageArchetype
//    {
//        CsvConfiguration ConfigCsv { get; }

//        void Write(DeckAverageArchetype deck, string directory);
//    }

//    public class WriterDeckAverageArchetype : IWriterDeckAverageArchetype
//    {
//        public CsvConfiguration ConfigCsv { get; private set; } = new CsvConfiguration(CultureInfo.InvariantCulture)
//        {
//            Delimiter = "\t",
//            HeaderValidated = null,
//            MissingFieldFound = null,
//        };

//        //IUtilLib util;
//        //public WriterDeckAverageArchetype(IUtilLib util)
//        //{
//        //    this.util = util;
//        //}
//        public WriterDeckAverageArchetype()
//        {
//        }

//        public void Write(DeckAverageArchetype deck, string directory)
//        {
//            Directory.CreateDirectory(directory);
//            var f = Path.Combine(directory, $"{deck.Name}.txt");
//            using (var writer = new CsvWriter(new StreamWriter(f), ConfigCsv))
//            {
//                var records = deck.Cards.All.Select(i =>
//                    new DeckCardRawModel
//                    {
//                        Qty = i.Amount,
//                        Code = i.Card.name,
//                        IsSideboard = i.Zone == DeckCardZoneEnum.Sideboard,
//                        IsMainOther = false
//                    })
//                    .Union(deck.CardsMainOther.Select(i =>
//                    new DeckCardRawModel
//                    {
//                        Qty = 0,
//                        Code = i.name,
//                        IsSideboard = false,
//                        IsMainOther = true
//                    })
//                );

//                //if (records == null)
//                //    System.Diagnostics.Debugger.Break();

//                writer.WriteRecords(records);
//            }
//        }
//    }
//}