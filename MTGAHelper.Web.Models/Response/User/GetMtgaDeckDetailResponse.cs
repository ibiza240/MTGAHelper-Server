﻿using MTGAHelper.Entity.MtgaDeckStats;
using MTGAHelper.Web.Models.Response.Deck;
using MTGAHelper.Web.Models.Response.User.History;
using System.Collections.Generic;

namespace MTGAHelper.Web.Models.Response.User
{
    public class GetMtgaDeckDetailResponse
    {
        public MtgaDeckDetailDto Detail { get; set; }

        public GetMtgaDeckDetailResponse(MtgaDeckDetailDto detail)
        {
            Detail = detail;
        }
    }

    public class MtgaDeckDetailDto
    {
        public string DeckId { get; set; }
        public string DeckImage { get; set; }
        public string DeckName { get; set; }
        public string DeckColor { get; set; }

        //public string FirstPlayed { get; set; }
        //public string LastPlayed { get; set; }
        // public ICollection<MtgaDeckStatsByFormat> StatsByFormat { get; set; }

        public ICollection<MatchDtoLightweight> Matches { get; set; }
        public ICollection<DeckCardDto> CardsMain { get; set; }
        public ICollection<KeyValuePair<string, DeckCardDto[]>> CardsNotMainByZone { get; set; }

        public ICollection<DeckManaCurveDto> ManaCurve { get; set; }
    }
}