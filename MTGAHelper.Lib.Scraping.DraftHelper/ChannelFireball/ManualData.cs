using MTGAHelper.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib.Scraping.DraftHelper.ChannelFireball
{
    public partial class ManualData
    {
        public static readonly Dictionary<string, string> colors = new Dictionary<string, string> {
            {"W", "white" },
            {"U", "blue" },
            {"B", "black" },
            {"R", "red" },
            {"G", "green" },
        };

        public Dictionary<string, UrlToScrapeModel> modelBySet = new Dictionary<string, UrlToScrapeModel>()
        {
            { "THB", new UrlToScrapeModel("theros-beyond-death", colors.And("gold", "gold-and-artifacts")) },
            { "ELD", new UrlToScrapeModel("throne-of-eldraine", colors.And("gold", "artifacts-and-gold-cards").And("lands", "artifacts-and-lands")) },
            { "M20", new UrlToScrapeModel("core-set-2020", colors.And("", "gold-artifacts-and-lands")) },
            { "WAR", new UrlToScrapeModel("war-of-the-spark", colors.And("", "gold-artifacts-and-lands")) },
            { "RNA", new UrlToScrapeModel("ravnica-allegiance", colors.And(new Dictionary<string, string>
            {
                { "WU", "azorius" },
                { "RG", "gruul" },
                { "WB", "orzhov" },
                { "BR", "rakdos" },
                { "UG", "simic-and-colorless" },
            })) },
            { "GRN", new UrlToScrapeModel("guilds-of-ravnica", colors.And(new Dictionary<string, string>
            {
                { "WR", "boros" },
                { "UB", "dimir" },
                { "BG", "golgari" },
                { "UR", "izzet" },
                { "WG", "selesnya" },
                { "", "artifacts-lands-and-guild-ranking" },
            })) },
            { "DOM", new UrlToScrapeModel("dominaria", colors.And("gold", "gold-artifacts-and-lands")) },
        };
        public Dictionary<string, ICollection<DraftRating>> manualRatings = new Dictionary<string, ICollection<DraftRating>>
            {
                { "THB", new DraftRating[] {
                    new DraftRating
                    {
                        CardName = "Temple of Abandon",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "These are all solid playables, but rarely worth taking over a card your deck needs. They go up in value if you’re playing 3 colors, and I’d play a half-on Temple in most decks.",
                    },
                    new DraftRating
                    {
                        CardName = "Temple of Deceit",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "These are all solid playables, but rarely worth taking over a card your deck needs. They go up in value if you’re playing 3 colors, and I’d play a half-on Temple in most decks.",
                    },
                    new DraftRating
                    {
                        CardName = "Temple of Enlightenment",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "These are all solid playables, but rarely worth taking over a card your deck needs. They go up in value if you’re playing 3 colors, and I’d play a half-on Temple in most decks.",
                    },
                    new DraftRating
                    {
                        CardName = "Temple of Malice",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "These are all solid playables, but rarely worth taking over a card your deck needs. They go up in value if you’re playing 3 colors, and I’d play a half-on Temple in most decks.",
                    },
                    new DraftRating
                    {
                        CardName = "Temple of Plenty",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "These are all solid playables, but rarely worth taking over a card your deck needs. They go up in value if you’re playing 3 colors, and I’d play a half-on Temple in most decks.",
                    },
                }},
                { "ELD", new DraftRating[] {
                }},
                { "M20", new [] {
                    new DraftRating
                    {
                        CardName = "Bloodfell Caves",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "All of these lands are nice additions to any 2-color deck, and make splashing a third color much easier. I tend to take them a little higher than average playables, but under anything premium.",
                    },
                    new DraftRating
                    {
                        CardName = "Blossoming Sands",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "All of these lands are nice additions to any 2-color deck, and make splashing a third color much easier. I tend to take them a little higher than average playables, but under anything premium.",
                    },
                    new DraftRating
                    {
                        CardName = "Dismal Backwater",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "All of these lands are nice additions to any 2-color deck, and make splashing a third color much easier. I tend to take them a little higher than average playables, but under anything premium.",
                    },
                    new DraftRating
                    {
                        CardName = "Jungle Hollow",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "All of these lands are nice additions to any 2-color deck, and make splashing a third color much easier. I tend to take them a little higher than average playables, but under anything premium.",
                    },
                    new DraftRating
                    {
                        CardName = "Wind-Scarred Crag",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "All of these lands are nice additions to any 2-color deck, and make splashing a third color much easier. I tend to take them a little higher than average playables, but under anything premium.",
                    },
                    new DraftRating
                    {
                        CardName = "Rugged Highlands",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "All of these lands are nice additions to any 2-color deck, and make splashing a third color much easier. I tend to take them a little higher than average playables, but under anything premium.",
                    },
                    new DraftRating
                    {
                        CardName = "Scoured Barrens",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "All of these lands are nice additions to any 2-color deck, and make splashing a third color much easier. I tend to take them a little higher than average playables, but under anything premium.",
                    },
                    new DraftRating
                    {
                        CardName = "Swiftwater Cliffs",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "All of these lands are nice additions to any 2-color deck, and make splashing a third color much easier. I tend to take them a little higher than average playables, but under anything premium.",
                    },
                    new DraftRating
                    {
                        CardName = "Thornwood Falls",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "All of these lands are nice additions to any 2-color deck, and make splashing a third color much easier. I tend to take them a little higher than average playables, but under anything premium.",
                    },
                    new DraftRating
                    {
                        CardName = "Tranquil Cove",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "All of these lands are nice additions to any 2-color deck, and make splashing a third color much easier. I tend to take them a little higher than average playables, but under anything premium.",
                    },
                    new DraftRating
                    {
                        CardName = "Temple of Triumph",
                        RatingValue = 3.5f, RatingToDisplay = "3.5",
                        Description = "Scry 1 is a real advantage, and I’d take these Temples aggressively. They are rare, so it doesn’t come up a ton, but treat them like real playables (and play them even when half on color).",
                    },
                    new DraftRating
                    {
                        CardName = "Temple of Mystery",
                        RatingValue = 3.5f, RatingToDisplay = "3.5",
                        Description = "Scry 1 is a real advantage, and I’d take these Temples aggressively. They are rare, so it doesn’t come up a ton, but treat them like real playables (and play them even when half on color).",
                    },
                    new DraftRating
                    {
                        CardName = "Temple of Silence",
                        RatingValue = 3.5f, RatingToDisplay = "3.5",
                        Description = "Scry 1 is a real advantage, and I’d take these Temples aggressively. They are rare, so it doesn’t come up a ton, but treat them like real playables (and play them even when half on color).",
                    },
                    new DraftRating
                    {
                        CardName = "Temple of Epiphany",
                        RatingValue = 3.5f, RatingToDisplay = "3.5",
                        Description = "Scry 1 is a real advantage, and I’d take these Temples aggressively. They are rare, so it doesn’t come up a ton, but treat them like real playables (and play them even when half on color).",
                    },
                    new DraftRating
                    {
                        CardName = "Temple of Malady",
                        RatingValue = 3.5f, RatingToDisplay = "3.5",
                        Description = "Scry 1 is a real advantage, and I’d take these Temples aggressively. They are rare, so it doesn’t come up a ton, but treat them like real playables (and play them even when half on color).",
                    }
                }},
                { "WAR", new DraftRating[] {
                }},
                { "RNA", new DraftRating[] {
                    new DraftRating
                    {
                        CardName = "Azorius Locket",
                        RatingValue = 2.0f, RatingToDisplay = "2.0",
                        Description = "Lockets get a bad rap. In slower decks, they are often worth a slot, as they help cast expensive spells and can be cracked for a pair of cards later. The non-red ones are better, as the red decks are often lower to the ground, but all of them have their uses.",
                    },
                    new DraftRating
                    {
                        CardName = "Gruul Locket",
                        RatingValue = 2.0f, RatingToDisplay = "2.0",
                        Description = "Lockets get a bad rap. In slower decks, they are often worth a slot, as they help cast expensive spells and can be cracked for a pair of cards later. The non-red ones are better, as the red decks are often lower to the ground, but all of them have their uses.",
                    },
                    new DraftRating
                    {
                        CardName = "Orzhov Locket",
                        RatingValue = 2.0f, RatingToDisplay = "2.0",
                        Description = "Lockets get a bad rap. In slower decks, they are often worth a slot, as they help cast expensive spells and can be cracked for a pair of cards later. The non-red ones are better, as the red decks are often lower to the ground, but all of them have their uses.",
                    },
                    new DraftRating
                    {
                        CardName = "Rakdos Locket",
                        RatingValue = 2.0f, RatingToDisplay = "2.0",
                        Description = "Lockets get a bad rap. In slower decks, they are often worth a slot, as they help cast expensive spells and can be cracked for a pair of cards later. The non-red ones are better, as the red decks are often lower to the ground, but all of them have their uses.",
                    },
                    new DraftRating
                    {
                        CardName = "Simic Locket",
                        RatingValue = 2.0f, RatingToDisplay = "2.0",
                        Description = "Lockets get a bad rap. In slower decks, they are often worth a slot, as they help cast expensive spells and can be cracked for a pair of cards later. The non-red ones are better, as the red decks are often lower to the ground, but all of them have their uses.",
                    },
                    new DraftRating
                    {
                        CardName = "Azorius Guildgate",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are two colors, I like picking up Gates over solid playables, but don’t heavily prioritize them. In the Gates deck, they are clearly high picks, and often better than shocklands even.",
                    },
                    new DraftRating
                    {
                        CardName = "Gruul Guildgate",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are two colors, I like picking up Gates over solid playables, but don’t heavily prioritize them. In the Gates deck, they are clearly high picks, and often better than shocklands even.",
                    },
                    new DraftRating
                    {
                        CardName = "Orzhov Guildgate",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are two colors, I like picking up Gates over solid playables, but don’t heavily prioritize them. In the Gates deck, they are clearly high picks, and often better than shocklands even.",
                    },
                    new DraftRating
                    {
                        CardName = "Rakdos Guildgate",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are two colors, I like picking up Gates over solid playables, but don’t heavily prioritize them. In the Gates deck, they are clearly high picks, and often better than shocklands even.",
                    },
                    new DraftRating
                    {
                        CardName = "Simic Guildgate",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are two colors, I like picking up Gates over solid playables, but don’t heavily prioritize them. In the Gates deck, they are clearly high picks, and often better than shocklands even.",
                    },
                    new DraftRating
                    {
                        CardName = "Hallowed Fountain",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are two colors, I like picking up Gates over solid playables, but don’t heavily prioritize them. In the Gates deck, they are clearly high picks, and often better than shocklands even.",
                    },
                    new DraftRating
                    {
                        CardName = "Stomping Ground",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are two colors, I like picking up Gates over solid playables, but don’t heavily prioritize them. In the Gates deck, they are clearly high picks, and often better than shocklands even.",
                    },
                    new DraftRating
                    {
                        CardName = "Godless Shrine",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are two colors, I like picking up Gates over solid playables, but don’t heavily prioritize them. In the Gates deck, they are clearly high picks, and often better than shocklands even.",
                    },
                    new DraftRating
                    {
                        CardName = "Blood Crypt",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are two colors, I like picking up Gates over solid playables, but don’t heavily prioritize them. In the Gates deck, they are clearly high picks, and often better than shocklands even.",
                    },
                    new DraftRating
                    {
                        CardName = "Breeding Pool",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are two colors, I like picking up Gates over solid playables, but don’t heavily prioritize them. In the Gates deck, they are clearly high picks, and often better than shocklands even.",
                    },
                }},
                { "GRN", new DraftRating[] {
                    new DraftRating
                    {
                        CardName = "Dimir Locket",
                        RatingValue = 2.0f, RatingToDisplay = "2.0",
                        Description = "I’ve played a few Sealeds, and Lockets have been… fine. They are playable if you have a medium to high curve, and it is nice that you can crack them for two cards later in the game, but I’m still not thrilled to spend 3 mana on them early (which makes them dubious acceleration). I like these quite a bit more than any other 3-mana rocks in the past, so I’m curious to see how people rate them in a month.",
                    },
                    new DraftRating
                    {
                        CardName = "Golgari Locket",
                        RatingValue = 2.0f, RatingToDisplay = "2.0",
                        Description = "I’ve played a few Sealeds, and Lockets have been… fine. They are playable if you have a medium to high curve, and it is nice that you can crack them for two cards later in the game, but I’m still not thrilled to spend 3 mana on them early (which makes them dubious acceleration). I like these quite a bit more than any other 3-mana rocks in the past, so I’m curious to see how people rate them in a month.",
                    },
                    new DraftRating
                    {
                        CardName = "Izzet Locket",
                        RatingValue = 2.0f, RatingToDisplay = "2.0",
                        Description = "I’ve played a few Sealeds, and Lockets have been… fine. They are playable if you have a medium to high curve, and it is nice that you can crack them for two cards later in the game, but I’m still not thrilled to spend 3 mana on them early (which makes them dubious acceleration). I like these quite a bit more than any other 3-mana rocks in the past, so I’m curious to see how people rate them in a month.",
                    },
                    new DraftRating
                    {
                        CardName = "Selesnya Locket",
                        RatingValue = 2.0f, RatingToDisplay = "2.0",
                        Description = "I’ve played a few Sealeds, and Lockets have been… fine. They are playable if you have a medium to high curve, and it is nice that you can crack them for two cards later in the game, but I’m still not thrilled to spend 3 mana on them early (which makes them dubious acceleration). I like these quite a bit more than any other 3-mana rocks in the past, so I’m curious to see how people rate them in a month.",
                    },
                    new DraftRating
                    {
                        CardName = "Boros Guildgate",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are 2 colors, I’d take Guildgates reasonably high, over most commons of the same level (especially once you have enough playables). If you have any of the uncommon cycle that costs AABB, these go up a notch, and once you’re 3+ colors they definitely go up. In general, most people probably take these too low, so it’s possible you should think of them as a 3.25, if that helps.",
                    },
                    new DraftRating
                    {
                        CardName = "Dimir Guildgate",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are 2 colors, I’d take Guildgates reasonably high, over most commons of the same level (especially once you have enough playables). If you have any of the uncommon cycle that costs AABB, these go up a notch, and once you’re 3+ colors they definitely go up. In general, most people probably take these too low, so it’s possible you should think of them as a 3.25, if that helps.",
                    },
                    new DraftRating
                    {
                        CardName = "Golgari Guildgate",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are 2 colors, I’d take Guildgates reasonably high, over most commons of the same level (especially once you have enough playables). If you have any of the uncommon cycle that costs AABB, these go up a notch, and once you’re 3+ colors they definitely go up. In general, most people probably take these too low, so it’s possible you should think of them as a 3.25, if that helps.",
                    },
                    new DraftRating
                    {
                        CardName = "Izzet Guildgate",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are 2 colors, I’d take Guildgates reasonably high, over most commons of the same level (especially once you have enough playables). If you have any of the uncommon cycle that costs AABB, these go up a notch, and once you’re 3+ colors they definitely go up. In general, most people probably take these too low, so it’s possible you should think of them as a 3.25, if that helps.",
                    },
                    new DraftRating
                    {
                        CardName = "Selesnya Guildgate",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "If you are 2 colors, I’d take Guildgates reasonably high, over most commons of the same level (especially once you have enough playables). If you have any of the uncommon cycle that costs AABB, these go up a notch, and once you’re 3+ colors they definitely go up. In general, most people probably take these too low, so it’s possible you should think of them as a 3.25, if that helps.",
                    },
                }},

                //{ "XLN", new DraftRating[] {
                //}},
                //{ "RIX", new DraftRating[] {
                //}},
                { "DOM", new DraftRating[] {
                    new DraftRating
                    {
                        CardName = "Clifftop Retreat",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "These are all great if you’re in both colors, and enable splashes. Don’t take them highly, but always play them if you get them.",
                    },
                    new DraftRating
                    {
                        CardName = "Hinterland Harbor",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "These are all great if you’re in both colors, and enable splashes. Don’t take them highly, but always play them if you get them.",
                    },
                    new DraftRating
                    {
                        CardName = "Isolated Chapel",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "These are all great if you’re in both colors, and enable splashes. Don’t take them highly, but always play them if you get them.",
                    },
                    new DraftRating
                    {
                        CardName = "Sulfur Falls",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "These are all great if you’re in both colors, and enable splashes. Don’t take them highly, but always play them if you get them.",
                    },
                    new DraftRating
                    {
                        CardName = "Woodland Cemetery",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "These are all great if you’re in both colors, and enable splashes. Don’t take them highly, but always play them if you get them.",
                    },
                    new DraftRating
                    {
                        CardName = "Memorial to Folly",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "I like all of these, and would take them over midrange playables any day. They all provide solid advantages later in the game, and it’s well worth the risk of your land entering tapped on a crucial turn.",
                    },
                    new DraftRating
                    {
                        CardName = "Memorial to Genius",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "I like all of these, and would take them over midrange playables any day. They all provide solid advantages later in the game, and it’s well worth the risk of your land entering tapped on a crucial turn.",
                    },
                    new DraftRating
                    {
                        CardName = "Memorial to Glory",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "I like all of these, and would take them over midrange playables any day. They all provide solid advantages later in the game, and it’s well worth the risk of your land entering tapped on a crucial turn.",
                    },
                    new DraftRating
                    {
                        CardName = "Memorial to Unity",
                        RatingValue = 3.0f, RatingToDisplay = "3.0",
                        Description = "I like all of these, and would take them over midrange playables any day. They all provide solid advantages later in the game, and it’s well worth the risk of your land entering tapped on a crucial turn.",
                    },
                }},
                //{ "M19", new DraftRating[] {
                //}},
            };
    }
}
