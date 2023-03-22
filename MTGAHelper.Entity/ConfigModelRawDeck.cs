﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Entity
{
    public class DeckCardRaw
    {
        public int GrpId { get; set; }
        public int Amount { get; set; }
        public DeckCardZoneEnum Zone { get; set; }
    }

    public class ConfigModelRawDeck
    {
        public string Id { get; set; }
        public int DeckTileId { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public string ArchetypeId { get; set; }

        [Obsolete("Will be phased out")]
        public Dictionary<int, int> CardsMain { get; set; } = new Dictionary<int, int>();

        [Obsolete("Will be phased out")]
        public Dictionary<int, int> CardsSideboard { get; set; } = new Dictionary<int, int>();

        [Obsolete("Will be phased out")]
        public int CardCommander { get; set; }

        public ICollection<DeckCardRaw> Cards { get; set; }

        [JsonIgnore]
        public ICollection<DeckCardRaw> CardsMainWithCommander
        {
            get
            {
                try
                {
                    return Cards
                        .Where(i => i.Zone == DeckCardZoneEnum.Deck || i.Zone == DeckCardZoneEnum.Commander)
                        .ToArray();
                }
                catch (Exception ex)
                {
                    //System.Diagnostics.Debugger.Break();
                    return default;
                }
            }
        }

        [JsonIgnore]
        public Dictionary<DeckCardZoneEnum, ICollection<DeckCardRaw>> CardsNotMainByZone
        {
            get
            {
                try
                {
                    return Cards
                        .Where(i => i.Zone != DeckCardZoneEnum.Deck)
                        .GroupBy(i => i.Zone)
                        .ToDictionary(i => i.Key, i => (ICollection<DeckCardRaw>)i.ToArray());
                }
                catch (Exception ex)
                {
                    //System.Diagnostics.Debugger.Break();
                    return default;
                }
            }
        }

        public string Description { get; set; }
        public string CardBack { get; set; }

        public ICollection<DeckCard> ToDeckCards(IReadOnlyDictionary<int, Card> allCards)
        {
            try
            {
                return Cards
                .Select(i => new DeckCard(allCards[i.GrpId], i.Amount, i.Zone))
                .ToArray();
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debugger.Break();
                return Array.Empty<DeckCard>();
            }
        }
    }
}