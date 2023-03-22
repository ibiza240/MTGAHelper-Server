using MTGAHelper.Web.Models.SharedDto;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Web.Models.Response.User.History
{
    public class EconomyEventDto
    {
        public string Context { get; set; }
        public DateTime DateTime { get; set; }
        public ICollection<EconomyEventChangeDto> Changes { get; set; } = Array.Empty<EconomyEventChangeDto>();
        public ICollection<CardWithAmountDto> NewCards { get; set; } = Array.Empty<CardWithAmountDto>();
    }

    public class EconomyEventChangeDto
    {
        public dynamic Amount { get; set; }
        public string Type { get; set; }
        public string Value { get; set; } = "";
    }
}