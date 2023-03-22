using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MTGAHelper.Entity.CollectionDecksCompare;
using MTGAHelper.Lib;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Web.Models.Response;
using MTGAHelper.Web.Models.Response.Dashboard;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Web.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : MtgaHelperControllerBase
    {
        private readonly IMapper mapper;
        private readonly UtilColors utilColors;

        public DashboardController(
            CacheSingleton<IReadOnlySet<string>> cacheMembers,
            ISessionContainer container,
            IConfigManagerUsers configUsers,
            IMapper mapper,
            UtilColors utilColors
            )
            : base(cacheMembers, container, configUsers)
        {
            this.mapper = mapper;
            this.utilColors = utilColors;
        }

        // GET api/dashboard/summary
        [HttpGet("summary")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IResponse>> GetDashboardSummary()
        {
            var e = await CheckUser();
            if (e != null) return e;

            var data = container.GetUserDashboardSummary(userId);

            var summary = mapper.Map<Dictionary<string, DashboardModelSummaryDto>>(data);
            return Ok(summary.ToArray());
        }

        // GET api/dashboard/detail
        [HttpGet("detail")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IResponse>> GetDashboardDetail()
        {
            var e = await CheckUser();
            if (e != null) return e;

            var data = container
                .GetUserDashboardDetails(userId)
                .Where(card => BannedCardsProviderTemp.GetBannedCards(BannedCardFormat.Historic).Contains(card.CardName) == false)
                .ToArray();

            var details = mapper.Map<CardMissingDetailsModelResponseDto[]>(data);
            return Ok(details);
        }

        // GET api/dashboard/decksByCard?card=xyz
        [HttpGet("decksbycard")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> Get([FromQuery] string card)
        {
            var e = await CheckUser();
            if (e != null) return e;

            var cardFound = container.AllCards.CardsByName(card).LastOrDefault();
            if (cardFound == null)
                return NotFound(new ErrorResponse($"Card '{card}' not found."));

            Log.Information("User {userId} wants to get decks that use the card {cardName}", userId, card);

            var allDecks = await container.GetDecksIncludingUserCustom(userId);

            try
            {
                var decksInfo = Array.Empty<KeyValuePair<string, CardRequiredInfoByDeck>>();
                var userData = configUsers.Get(userId).DataInMemory;
                if (userData.CompareResult.ByCard.ContainsKey(cardFound.Name))
                {
                    var decksMissing = userData.CompareResult
                       .ByCard[cardFound.Name].ByDeck
                       .Where(i => allDecks.ContainsKey(i.Key) == false)
                       .ToArray();
                    foreach (var d in decksMissing)
                    {
                        Log.Warning("User {userId} - Deck found in compareResult not found in allDecks: {deckId}", userId, d.Key);
                    }

                    decksInfo = userData.CompareResult
                       .ByCard[cardFound.Name].ByDeck
                       .Where(i => allDecks.ContainsKey(i.Key))
                       .OrderByDescending(i => i.Value.NbRequiredMain)
                       .ThenByDescending(i => i.Value.NbRequiredSideboard)
                       .ThenBy(i => allDecks[i.Key].Deck.Name)
                       //.Select(i => $"[{i.Value.ByCard[c].NbRequiredMain}, {i.Value.ByCard[c].NbRequiredSideboard}]x in {container.GetDecksIncludingUserCustom(userId)[i.Key].Name}")
                       .ToArray();
                }
                else
                {
                    Log.Error("decksByCard - User {userId} - Card {cardName} was not found in compare results by card",
                        userId, cardFound.Name);
                }

                var decks = allDecks
                    .ToDictionary(i => i.Key, i => i.Value.Deck);

                return Ok(DashboardDetailsCardResponse.Build(userId, allDecks, cardFound, decks, decksInfo, utilColors));
            }
            catch (KeyNotFoundException ex)
            {
                Log.Fatal(ex, "Unexpected error (User id: {userId})!", userId);
                return Ok(new DashboardDetailsCardResponse());
            }
        }
    }
}