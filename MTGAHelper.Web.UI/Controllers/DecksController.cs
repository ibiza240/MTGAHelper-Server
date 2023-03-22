using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MTGAHelper.Entity;
using MTGAHelper.Lib;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Lib.Exceptions;
using MTGAHelper.Lib.Scraping.DeckSources.Aetherhub;
using MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish;
using MTGAHelper.Web.Models;
using MTGAHelper.Web.Models.Request;
using MTGAHelper.Web.Models.Response;
using MTGAHelper.Web.Models.Response.Deck;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Web.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DecksController : MtgaHelperControllerBase
    {
        private readonly DeckScraperAetherhubByUrl scraperAetherhub;
        private readonly DeckScraperMtgGoldfishByUrl scraperMtgGoldfish;
        private readonly IMapper mapper;
        private readonly UtilManaCurve utilManaCurve;
        private readonly DecksFinderByCards decksFinderByCards;

        public DecksController(
            CacheSingleton<IReadOnlySet<string>> cacheMembers,
            ISessionContainer container,
            IConfigManagerUsers configUsers,
            DeckScraperAetherhubByUrl scraperAetherhub, DeckScraperMtgGoldfishByUrl scraperMtgGoldfish,
            IMapper mapper,
            UtilManaCurve utilManaCurve,
            DecksFinderByCards decksFinderByCards
            )
            : base(cacheMembers, container, configUsers)
        {
            this.scraperAetherhub = scraperAetherhub;
            this.scraperMtgGoldfish = scraperMtgGoldfish;
            this.mapper = mapper;
            this.utilManaCurve = utilManaCurve;
            this.decksFinderByCards = decksFinderByCards;
        }

        // GET api/decks
        [HttpGet]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> Get([FromQuery] string card)
        {
            (var totalDecks, var decks) = container.GetSystemDecks(userId, new FilterDeckParams { Card = card });

            var decksDto = mapper.Map<ICollection<DeckSummaryResponseDto>>(decks);
            return Ok(new DeckListResponse<DeckSummaryResponseDto>(totalDecks, decksDto));
        }

        // GET api/decks/xyz
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetDeckById([FromRoute] string id)
        {
            await CheckUser();

            var deckInfo = await container.GetDeck(userId, id);
            if (deckInfo == null)
            {
                Log.Error("Deck {deckId} not found, please refresh the webapp (F5)", id);
                return NotFound(new ErrorResponse($"Deck not found, please refresh the webapp (F5)"));
            }

            return Ok(DeckResponse.FromDeckInfo(deckInfo, utilManaCurve, mapper));
        }

        // GET api/decks/byhash/xyz
        [HttpGet("byhash/{hash}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> GetDeckByHash([FromRoute] uint hash)
        {
            await CheckUser();

            Log.Information("GetDeckByHash {userId} {ipAddress}",
                userId,
                GetRequestIP());

            var deckInfo = await container.GetDeckByHash(userId, hash);
            if (deckInfo == null)
            {
                Log.Error("Deck by hash {hash} not found", hash);
                return NotFound(new ErrorResponse($"Deck by hash {hash} not found"));
            }

            return Ok(DeckResponse.FromDeckInfo(deckInfo, utilManaCurve, mapper));
        }

        // POST api/decks/fromurl
        [HttpPost("fromurl")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ActionResult<IResponse> GetDeckFromUrl([FromBody] DeckFromUrlRequest body)
        {
            try
            {
                (string name, string mtgaFormat) info = (null, null);

                if (body.Url.ToLower().Contains("aetherhub"))
                    info = scraperAetherhub.GetDeck(body.Url);
                else if (body.Url.ToLower().Contains("mtggoldfish"))
                    info = scraperMtgGoldfish.GetDeck(body.Url);

                return Ok(new DeckFromUrlResponse(info.name, info.mtgaFormat));
            }
            catch (InvalidOperationException)
            {
                return NotFound(new ErrorResponse($"The url {body.Url} is invalid"));
            }
            catch (InvalidDeckFormatException)
            {
                return NotFound(new ErrorResponse($"The deck from the url {body.Url} is invalid for the Standard format"));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Problem downloading deck at url: {url}", body.Url);
                return NotFound(new ErrorResponse("There was a problem logged with this url and we will investigate, sorry for the inconvenience"));
            }
        }

        // POST api/decks/byhash/xyz
        [HttpPost("bycards")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IResponse>> GetDecksByCards([FromBody] ICollection<string> cards)
        {
            Log.Information("GetDecksByCards {userId} {ipAddress}",
                userId,
                GetRequestIP());

            await CheckUser();

            if (cards.Count < 3)
                return Ok(new { decks = Array.Empty<object>() });

            var results = decksFinderByCards.GetDecksByCards(cards);

#if DEBUG && !DEBUGWITHSERVER
            var server = "https://localhost:5001";
#else
        var server = "https://mtgahelper.com";
#endif
            var decks = results.Select(i => new { i.Name, Url = server + "/deck.html?id=" + Fnv1aHasher.To32BitFnv1aHash(i.Deck.Id) });
            return Ok(new { decks });
        }
    }
}