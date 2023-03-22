﻿using MTGAHelper.Entity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using MTGAHelper.Entity.CollectionDecksCompare;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Lib;

namespace MTGAHelper.Web.Models.Response.Dashboard
{
    public class DashboardDetailsCardResponse
    {
        public ICollection<DashboardDetailsCardDto> InfoByDeck { get; set; } = Array.Empty<DashboardDetailsCardDto>();

        //public DashboardDetailsCardResponse()
        //{
        //}

        public static DashboardDetailsCardResponse Build(string userId, Dictionary<string, ConfigModelDeck> dictDecks,
            Card c, Dictionary<string, IDeck> decks, KeyValuePair<string, CardRequiredInfoByDeck>[] decksInfo,
            UtilColors utilColors)
        {
            var ret = new DashboardDetailsCardResponse();

            var missing = decksInfo.Where(i => decks.ContainsKey(i.Key) == false).ToArray();
            if (missing.Any())
            {
                Log.Error("User {userId} Error in DashboardDetailsCardResponse: Cannot find these decks: <{ids}>. Original ids: <{ids2}>",
                    userId, string.Join(",", missing.Select(x => x.Key)), string.Join(",", missing.Select(x => x.Value.DeckId)));
            }

            ret.InfoByDeck = decksInfo
                .Where(i => missing.Any(x => x.Key == i.Key) == false)
                .Select(i => new DashboardDetailsCardDto
                {
                    DeckId = i.Key,
                    DeckName = decks[i.Key].Name,
                    NbMain = i.Value.ByCard[c.Name].NbRequiredMain,
                    NbSideboard = i.Value.ByCard[c.Name].NbRequiredSideboard,
                    DeckColor = utilColors.FromDeck(decks[i.Key]),
                    DeckDateCreated = dictDecks[i.Key].DateCreatedUtc,
                    DeckScraperTypeId = decks[i.Key].ScraperType.Id,
                })
                .ToArray();

            return ret;
        }
    }

    public class DashboardDetailsCardDto
    {
        public string DeckName { get; set; }
        public string DeckId { get; set; }
        public int NbMain { get; set; }
        public int NbSideboard { get; set; }

        public string DeckScraperTypeId { get; set; }
        public string DeckColor { get; set; }
        public DateTime? DeckDateCreated { get; set; }
    }
}