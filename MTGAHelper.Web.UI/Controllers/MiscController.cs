using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Services;
using MTGAHelper.Lib;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.Config.Articles;
using MTGAHelper.Lib.Config.News;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Lib.OutputLogParser;
using MTGAHelper.Web.Models.Request;
using MTGAHelper.Web.Models.Response;
using MTGAHelper.Web.Models.Response.Misc;
using MTGAHelper.Web.UI.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Web.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MiscController : MtgaHelperControllerBase
    {
        private readonly IMapper mapper;
        private readonly CacheSingleton<IReadOnlyCollection<ConfigModelNews>> cacheNews;
        private readonly ConfigManagerArticles configManagerArticles;
        private readonly ConfigModelApp configReloadable;
        private readonly ConfigModelApp configApp;
        private readonly MessageWriter messageWriter;
        private readonly LandsPreferenceManager landsManager;
        private readonly IReadOnlyCollection<string> dateFormats;
        private readonly FilesHashManager filesHashManager;
        private readonly CacheSingleton<IReadOnlyCollection<ConfigModelCalendarItem>> cacheCalendar;
        private readonly BasicLandIdentifier basicLandIdentifier;
        private readonly IReadOnlyDictionary<int, Card> allCards;

        public MiscController(
            IOptionsMonitor<ConfigModelApp> configReloadable,
            CacheSingleton<IReadOnlySet<string>> cacheMembers,
            ISessionContainer container,
            IConfigManagerUsers configUsers,
            IMapper mapper,
            CacheSingleton<IReadOnlyCollection<ConfigModelNews>> cacheNews,
            ConfigManagerArticles configManagerArticles,
            ConfigModelApp configApp,
            MessageWriter messageWriter,
            LandsPreferenceManager landsManager,
            ICardRepository cardRepo,
            IPossibleDateFormats dateFormatsProvider,
            FilesHashManager filesHashManager,
            CacheSingleton<IReadOnlyCollection<ConfigModelCalendarItem>> cacheCalendar,
            BasicLandIdentifier basicLandIdentifier
            )
            : base(cacheMembers, container, configUsers)
        {
            this.mapper = mapper;
            this.configReloadable = configReloadable.CurrentValue;
            this.cacheNews = cacheNews;
            this.configManagerArticles = configManagerArticles;
            this.configApp = configApp;
            this.messageWriter = messageWriter;
            this.landsManager = landsManager;
            this.allCards = cardRepo;
            this.dateFormats = dateFormatsProvider.Formats;
            this.filesHashManager = filesHashManager;
            this.cacheCalendar = cacheCalendar;
            this.basicLandIdentifier = basicLandIdentifier;
        }

        // POST api/misc/message
        [HttpPost("message")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IResponse>> SendMessage([FromBody] PostMessageRequest body)
        {
            var e = await CheckUser();

            messageWriter.WriteMessage(userId, body.Message);

            return Ok(new StatusOkResponse());
        }

        // GET api/misc/changelog
        [HttpGet("changelog")]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> GetChangeLog()
        {
            return Ok(configReloadable.Changelog);
        }

        // GET api/misc/version
        [HttpGet("version")]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> GetVersion()
        {
            var timestampUtc = container.GetVersionUtc();

            return Ok(new GetVersionResponse { DateTimeUtc = timestampUtc });
        }

        // GET api/misc/versiontracker
        [HttpGet("versiontracker")]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> GetVersionTracker()
        {
            return Ok(new GetVersionTrackerResponse { Version = configReloadable.VersionTrackerClient });
        }

        // GET api/misc/trackerclientmessages
        [HttpGet("trackerclientmessages")]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> GetTrackerClientMessages()
        {
            return Ok(configApp.TrackerClientMessages);
        }

        // GET api/misc/news
        [HttpGet("news")]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> GetNews()
        {
            var news = cacheNews.Get().Take(40);
            return Ok(news);
        }

        // GET api/misc/calendar
        [HttpGet("calendar")]
        [ProducesResponseType(200)]
        public ActionResult<ICollection<GetCalendarResponse>> GetCalendar()
        {
            var cachedCalendar = cacheCalendar.Get().Take(20);
            var response = mapper.Map<ICollection<GetCalendarResponse>>(cachedCalendar);
            return Ok(response);
        }

        // GET api/misc/articles
        [HttpGet("articles")]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> GetArticles()
        {
            var articles = configManagerArticles.GetAll();
            return Ok(articles);
        }

        // GET api/misc/lands
        [HttpGet("lands")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IResponse>> GetLands()
        {
            var e = await CheckUser();

            var lands = landsManager.GetLandsList(userId);
            return Ok(new GetLandsResponse(lands));
        }

        // GET api/misc/sets
        [HttpGet("sets")]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> GetSetsInfo()
        {
            //var aa = container.allCards.Values.Where(i => i.name.StartsWith("Celestial")).ToArray();

            var test2 = container.AllCards.Values
                .Where(i => i.Set != null)
                .Where(i => configApp.InfoBySet.ContainsKey(i.Set))
                .Where(i => basicLandIdentifier.IsBasicLand(i) == false)
                .Where(i => i.IsToken == false)
                .Where(i => i.LinkedFaceType != enumLinkedFace.SplitCard)
                .Where(i => i.LinkedFaceType != enumLinkedFace.DFC_Front)
                .Where(i => i.IsFoundInBooster)
                .Where(i => i.Number?.StartsWith("GR") == false)     // i.e. Teferi, Vraska special arts
                .ToArray();

            //var qq = container.allCards.Values.Where(i => i.name.StartsWith("Divine Pu") || i.name.StartsWith("Kami of Transm")).ToArray();

            //var csvtest = test2
            //    .Where(i => i.set == "M19" && i.rarity.Equals("Common", StringComparison.InvariantCultureIgnoreCase))
            //    .Select(i => $"{i.name}\t{i.number}\t{i.grpId}");

            var test = test2
                .GroupBy(i => new {set = i.Set, rarity = i.RarityStr })
                .ToArray();

            // Group by by number fixed the case of cards like Firemind's Research (GRN Rare promo was given)

            var dict = test
                .Select(i => new SetInfo
                {
                    Formats = configApp.InfoBySet[i.Key.set].Formats,
                    Name = i.Key.set,
                    Rarity = i.Key.rarity,
                    TotalCards = i.GroupBy(x => x.Number).Count()
                })
                .ToArray();

            //var bb = container.allCards.Values.Where(i => i.notInBooster == false).Where(i => i.set == "GRN").Where(i => i.rarity == "Rare").ToArray();
            //var cc = test.Where(i => i.Key.set == "GRN" && i.Key.rarity == "Rare").SelectMany(i => i).ToArray();

            //var rrr = bb.Where(i => cc.Select(x => x.grpId).Contains(i.grpId) == false);

            //var ooo = bb.GroupBy(i => i.number).Where(ix => ix.Count() > 1).ToArray();

            var r = new GetSetsResponse() { Sets = dict };

            return Ok(r);
        }

        // GET api/misc/dateformats
        [HttpGet("dateformats")]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> GetDateFormats()
        {
            return Ok(new GetDateFormatsResponse(dateFormats));
        }

        // GET api/misc/cards
        [HttpGet("cards")]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> GetAllCards()
        {
            return Ok(new GetCardsResponse(allCards.Values));
        }

        // GET api/misc/filehash?id=allcardscached2&hash=123
        [HttpGet("filehash")]
        [ProducesResponseType(200)]
        public ActionResult<bool> GetDraftRatings([FromQuery] string id, [FromQuery] uint hash)
        {
            Enum.TryParse(id, out DataFileTypeEnum oEnum);
            var serverHash = filesHashManager.HashByType[oEnum];

            return Ok(hash == serverHash);
        }

        // POST api/misc/logremoteerror
        [HttpPost("logremoteerror")]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> LogRemoteError([FromBody] PostLogRemoteErrorRequest body)
        {
            var strError = body.Exception.Length > 2000 ? body.Exception.Substring(0, 2000) : body.Exception;
            Log.Error(new Exception(strError), "REMOTE ERROR: User:{userId} Type:{errorType}", body.UserId, body.ErrorType);

            //if (body.Filename != null)
            //{
            //}

            return Ok();
        }

        //// POST api/misc/logremoteerrorfile
        //[HttpPost("logremoteerrorfile")]
        //[ProducesResponseType(200)]
        //public ActionResult<IResponse> LogRemoteErrorFile(IFormFile fileOutputLog)
        //{
        //    userId = Request.Cookies["userId"];
        //    if (userId == null)
        //    {
        //        Log.Error("LogRemoteErrorFile with no userId [{ip}]", GetRequestIP());
        //        return Ok();
        //    }

        //    fileDeflator.SaveFile(userId, fileOutputLog, Path.Combine(configApp.FolderInvalidZips, "remoteErrors"));

        //    return Ok();
        //}
    }
}