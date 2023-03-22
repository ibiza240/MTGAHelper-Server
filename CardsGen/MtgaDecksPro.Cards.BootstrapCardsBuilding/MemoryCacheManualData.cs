using Microsoft.Extensions.Caching.Memory;
using Minmaxdev.Cache.Common.Config;
using Minmaxdev.Cache.Memory.Service;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity;
using Serilog;
using System.Collections.Generic;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding
{
    public class MemoryCacheManualData : MemoryCacheWrapper<CacheManualDataModel>
    {
        public MemoryCacheManualData(
            ILogger logger,
            IMemoryCache cache,
            ICacheExpirationConfig<CacheManualDataModel> config
            )
            : base(logger, cache, config)
        {
            this.Set(new CacheManualDataModel
            {
                Cards = new Dictionary<string, CardManualData>
                {
                    {
                        "Hallowed Priest",
                        new CardManualData
                        {
                            ManaCost = "{2}{W}",
                            ImageCardUrl = "https://mtgarena.pro/mtg/pict/mtga/card_69109_EN.png",
                            OracleText = "When Hallowed Priest enters the battlefield, create a 1/1 white Spirit creature token with flying.",
                            SetScryfall = "ana",
                            TypeLine = "Creature — Human Cleric"
                        }
                    },
                    {
                        "Trapped in a Whirlpool",
                        new CardManualData
                        {
                            ManaCost = "{3}{U}",
                            ImageCardUrl = "https://mtgarena.pro/mtg/pict/mtga/card_69114_EN.png",
                            OracleText = "Enchant Creature\nEnchanted creature doesn't untap during its controller's untap step.",
                            SetScryfall = "ana",
                            TypeLine = "Enchantment"
                        }
                    },
                    {
                        "Abundant Falls",
                        new CardManualData
                        {
                        }
                    },
                    {
                        "Compound Fracture",
                        new CardManualData
                        {
                        }
                    },
                    {
                        "Mardu Outrider",
                        new CardManualData
                        {
                        }
                    },
                    {
                        "Hurloon Minotaur",
                        new CardManualData
                        {
                        }
                    },
                    {
                        "Tin Street Cadet",
                        new CardManualData
                        {
                        }
                    },
                    {
                        "Baloth Packhunter",
                        new CardManualData
                        {
                        }
                    },
                    {
                        "Treetop Recluse",
                        new CardManualData
                        {
                        }
                    },
                    //{
                    //    "Treasure",
                    //    new CardManualData
                    //    {
                    //        ManaCost = "",
                    //        ImageCardUrl = "https://img.scryfall.com/cards/border_crop/front/e/6/e6fa7d35-9a7a-40fc-9b97-b479fc157ab0.jpg?1568003548",
                    //        OracleText = "{T}, Sacrifice this artifact: Add one mana of any color.",
                    //        SetScryfall = "ana",
                    //        TypeLine = "Token Artifact — Treasure"
                    //    }
                    //},
                },

                //SetsGatherer = new Dictionary<string, string>
                //{
                //    { "MIR", "MI" },
                //    { "PLS", "PS" },
                //    { "ODY", "OD" },
                //    { "WTH", "WL" },
                //    { "INV", "IN" },
                //    { "MED", "MPS_GRN" },
                //}
            });
        }
    }
}