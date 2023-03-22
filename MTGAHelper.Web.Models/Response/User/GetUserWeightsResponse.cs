using MTGAHelper.Entity;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Web.Models.Response.User
{
    public class GetUserWeightsResponse
    {
        public Dictionary<string, UserWeightDto> Weights { get; set; }

        public GetUserWeightsResponse()
        {
        }

        public GetUserWeightsResponse(IReadOnlyDictionary<RarityEnum, UserWeightDto> weights)
        {
            Weights = weights.ToDictionary(i => i.Key.ToString(), i => i.Value);
        }
    }
}