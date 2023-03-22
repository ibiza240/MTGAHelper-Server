using CsvHelper;
using CsvHelper.Configuration;
using MtgaDecksPro.Cards.Entity.Config;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MtgaDecksPro.Tools.Internal.Service
{
    public class CommandDownloadMtgaAssets : ICommand
    {
        private readonly ILogger logger;
        private string folderData;

        public CommandDownloadMtgaAssets(
            ILogger logger,
            IConfigFolderDataCards configFolderDataCards
            )
        {
            this.folderData = configFolderDataCards.FolderDataCards;
            this.logger = logger;
        }

        public Task Execute()
        {
            if (File.Exists(@"..\..\..\..\cards\cardsbuilder\cards_localization.csv") == false)
            {
                Console.WriteLine("To manually generate the required cards_localization.csv:");
                Console.WriteLine(@"- Open Raw_CardDatabase_xyz with SqLite DB Browser from C:\Program Files\Wizards of the Coast\MTGA\MTGA_Data\Downloads\Raw");
                Console.WriteLine("- Execute SQL SELECT LocId, Formatted, enUS FROM Localizations");
                Console.WriteLine(@"- Save as CardsGen\cards\cardsbuilder\cards_localization.csv (with header, comma separated)");

                return Task.CompletedTask;
            }

            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
            };

            // Convert localizations from new csv to old mtga format
            using (var reader = new StreamReader(@"..\..\..\..\cards\cardsbuilder\cards_localization.csv"))
            using (var csv = new CsvReader(reader, config))
            {
                var test = csv.GetRecords<MtgaDataLocSqlite3Model>().ToArray();

                using (var writer = new StreamWriter(@"..\..\..\..\cards\cardsbuilder\data_loc.mtga"))
                    writer.Write(JsonSerializer.Serialize(new[] {
                        new {
                            isoCode =  "en-US",
                            keys = test.Where(i => i.Formatted == true).Select(i => new {
                                id = i.LocId,
                                text = i.enUS,
                            })
                        }
                    }));
            }

            // Convert cards from new csv to old mtga format
            using (var reader = new StreamReader(@"..\..\..\..\cards\cardsbuilder\cards_raw.csv"))
            using (var csv = new CsvReader(reader, config))
            {
                var dataCards = csv.GetRecords<MtgaDataCardSqlite3Model>().ToArray();

                foreach (var c in dataCards)
                {
                    c.Set = (c.ExpansionCode switch
                    {
                        "Y22" or "Y23" or "Y24" => $"Y{c.DigitalReleaseSet.Substring(c.DigitalReleaseSet.Length - 3)}",
                        _ => c.ExpansionCode,
                    }).ToLower();
                }

                using (var writer = new StreamWriter(@"..\..\..\..\cards\cardsbuilder\Raw_cards.mtga"))
                    writer.Write(JsonSerializer.Serialize(dataCards));
            }

            return Task.CompletedTask;
        }

        public class Asset
        {
            public string Name { get; set; }
            public int Length { get; set; }
            public int CompressedLength { get; set; }
        }

        public class RootObject
        {
            public List<Asset> Assets { get; set; }
        }
    }
}