using MTGAHelper.Lib.AllCards.MtgaDataCards;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MTGAHelper.Lib.AllCards
{
    public class MtgaToolCardsLoader
    {
        public ICollection<MtgaDataCardsRootObjectExtended> LoadTHB()
        {
            var filepathMtgaToolDatabase = Path.Combine(@"D:\repos\MTGAHelper\data", "database.json");
            var strDatabaseMtgaTool = File.ReadAllText(filepathMtgaToolDatabase);
            dynamic databaseMtgaTool = JToken.Parse(strDatabaseMtgaTool);
            // Check the cards only
            IDictionary<string, dynamic> cardsFromMtgaTool = ToDictionary(databaseMtgaTool.cards);

            var ret = cardsFromMtgaTool.Values
                .Where(i => i["set"] == "Theros: Beyond Death")
                .Select(i => new MtgaDataCardsRootObjectExtended
                {
                    grpid = Convert.ToInt32(i["id"]),
                    Name = i["name"],
                    isCollectible = i["collectible"],
                    //isCraftable = i["craftable"],
                    set = "THB",
                    CollectorNumber = i["cid"],
                    rarity = ConvertRarity(i["rarity"]),
                    colors = ConvertColor(i["cost"]),
                    colorIdentity = ConvertColor(i["cost"]),
                }).ToArray();

            return ret;
        }

        private int ConvertRarity(string r)
        {
            switch (r)
            {
                case "common":
                    return 2;

                case "uncommon":
                    return 3;

                case "rare":
                    return 4;

                case "mythic":
                    return 5;
            }
            return 0;
        }

        private List<int> ConvertColor(IEnumerable<object> colors)
        {
            var colorsClean = colors.Where(i => Regex.Match(i.ToString(), "[a-wyz]").Success).Distinct();

            var ret = colorsClean
                .Select(i =>
                {
                    switch (i)
                    {
                        case "w": return 1;
                        case "u": return 2;
                        case "b": return 3;
                        case "r": return 4;
                        case "g": return 5;
                    }
                    return 0;
                }).ToList();

            return ret;
        }

        private IDictionary<string, dynamic> ToDictionary(JObject @object)
        {
            var result = @object.ToObject<Dictionary<string, object>>();

            var JObjectKeys = (from r in result
                               let key = r.Key
                               let value = r.Value
                               where value.GetType() == typeof(JObject)
                               select key).ToList();

            var JArrayKeys = (from r in result
                              let key = r.Key
                              let value = r.Value
                              where value.GetType() == typeof(JArray)
                              select key).ToList();

            JArrayKeys.ForEach(key => result[key] = ((JArray)result[key]).Values().Select(x => ((JValue)x).Value).ToArray());
            JObjectKeys.ForEach(key => result[key] = ToDictionary(result[key] as JObject));

            return result;
        }
    }
}