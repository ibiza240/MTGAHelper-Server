﻿using MTGAHelper.Web.Models.Response.Deck;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Web.Models.SharedDto
{
    public class SimpleDeckDto
    {
        public string Name { get; set; } = "N/A";
        public ICollection<DeckCardDto> Cards { get; set; } = Array.Empty<DeckCardDto>();

        public string Colors { get; set; } = "";
        public string DeckImage { get; set; } = "";

        //internal string GetColors()
        //{
        //    var order = new Dictionary<char, int> {
        //        { 'W', 1 },
        //        { 'U', 2 },
        //        { 'B', 3 },
        //        { 'R', 4 },
        //        { 'G', 5 },
        //    };

        //    var colors = Main.Union(Sideboard).SelectMany(i => i.Color.ToArray()).Distinct();
        //    var colorsFinal = colors.OrderBy(i => order[i]);
        //    return string.Join("", colorsFinal);
        //}
    }
}