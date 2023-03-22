﻿using MTGAHelper.Entity;
using System.Collections.Generic;

namespace MTGAHelper.Web.Models.Response.Misc
{
    public class GetLandsResponse
    {
        public ICollection<CardLandPreferenceDto> Lands { get; set; }

        public GetLandsResponse(ICollection<CardLandPreferenceDto> lands)
        {
            Lands = lands;
        }
    }
}