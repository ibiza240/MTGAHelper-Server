using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Entity.DeckScraper;
using MTGAHelper.Lib;
using MTGAHelper.Lib.CacheLoaders;
using MTGAHelper.Lib.Config.Decks;
using MTGAHelper.Lib.Config.News;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Lib.JsonFixing;
using MTGAHelper.Lib.Scraping.DeckSources;
using MTGAHelper.Lib.Scraping.NewsScraper;
using MTGAHelper.Lib.WebTesterConcurrent;
using MTGAHelper.Web.Models.Response;
using MTGAHelper.Web.Models.Response.Deck;
using MTGAHelper.Web.UI.Model.Response.Admin;
using MTGAHelper.Web.UI.Shared;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Web.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private const string PASSWORD = "bLuNo";

        private readonly MessageWriter messageWriter;
        private readonly ISessionContainer container;
        private readonly IDecksDownloader decksDownloader;
        private readonly NewsDownloader newsDownloader;
        private readonly ConfigModelApp configApp;
        private readonly ConfigManagerDecks deckManager;
        private readonly Tester tester;
        private readonly CacheSingleton<IReadOnlyCollection<ConfigModelNews>> cacheNews;

        private readonly CacheSingleton<IReadOnlyDictionary<int, Card>> allCardsCache;
        private readonly IReadOnlyDictionary<string, AccountModel> dictAccounts;
        private readonly ActiveUserCounter activeUserCounter;
        private readonly MassJsonFileFixer massJsonFileFixer;
        private readonly FilesHashManager filesHashManager;
        private readonly CacheSingleton<IReadOnlySet<string>> cacheMembersEmail;
        private readonly CacheSingleton<IReadOnlyDictionary<string, DraftRatings>> cacheDraftRatings;
        private readonly ConfigUserCleaner configUserCleaner;
        private readonly CacheCalendarImageBinder cacheCalendarImageBinder;

        public AdminController(
            CacheSingleton<IReadOnlyCollection<ConfigModelNews>> cacheNews,
            ConfigModelApp configApp,
            ISessionContainer container,
            IDecksDownloader decksDownloader,
            NewsDownloader newsDownloader,
            MessageWriter messageWriter,
            CacheSingleton<IReadOnlyDictionary<int, Card>> allCardsCache,
            ConfigManagerDecks deckManager,
            Tester tester,
            CacheSingleton<IReadOnlyDictionary<string, AccountModel>> cacheAccounts,
            ActiveUserCounter activeUserCounter,
            MassJsonFileFixer massJsonFileFixer,
            FilesHashManager filesHashManager,
            CacheSingleton<IReadOnlySet<string>> cacheMembersEmail,
            CacheSingleton<IReadOnlyDictionary<string, DraftRatings>> cacheDraftRatings,
            ConfigUserCleaner configUserCleaner,
            CacheCalendarImageBinder cacheCalendarImageBinder
            )
        {
            this.deckManager = deckManager;
            this.cacheNews = cacheNews;
            this.configApp = configApp;
            this.messageWriter = messageWriter;
            this.container = container;
            this.decksDownloader = decksDownloader.Init(container.GetScraperUsers());
            this.newsDownloader = newsDownloader;
            this.tester = tester;
            this.allCardsCache = allCardsCache;
            //this.draftRatingsLoader = draftRatingsLoader;
            dictAccounts = cacheAccounts.Get();
            this.activeUserCounter = activeUserCounter;
            this.massJsonFileFixer = massJsonFileFixer;
            this.filesHashManager = filesHashManager;
            this.cacheMembersEmail = cacheMembersEmail;
            this.cacheDraftRatings = cacheDraftRatings;
            this.configUserCleaner = configUserCleaner;
            this.cacheCalendarImageBinder = cacheCalendarImageBinder;
        }

        // GET api/admin/news/update
        [HttpGet("news/update")]
        [ProducesResponseType(200)]
        public ActionResult<string> NewsUpdate([FromQuery] string password)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            newsDownloader.UpdateNewsList();

            return Ok(cacheNews.Get());
        }

        // GET api/admin/news/ignore
        [HttpGet("news/ignore")]
        [ProducesResponseType(200)]
        public ActionResult<string> NewsIgnore([FromQuery] string password, [FromQuery] string id, [FromQuery] bool undo = false)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ErrorResponse($"Parameter {nameof(id)} not provided"));

            var ids = id.Split(',');
            newsDownloader.Ignore(ids, undo);

            return Ok(cacheNews.Get());
        }

        // GET api/admin/activeusers
        [HttpGet("activeusers")]
        [ProducesResponseType(200)]
        public ActionResult<string> ActiveUsers([FromQuery] string password)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            var accountsGroupedByUserId = dictAccounts
                .Where(i => i.Value.MtgaHelperUserId != null)
                .GroupBy(i => i.Value.MtgaHelperUserId);

            //var test = accountsGroupedByUserId.Where(i => i.Count() > 1).ToArray();
            //var test2 = dictAccounts.Where(i => i.Value.MtgaHelperUserId == null).ToArray();

            var dictAccountsByUserId = accountsGroupedByUserId
                .ToDictionary(i => i.Key, i => i.First().Value.Email);

            var activeUsers = activeUserCounter.GetActiveUserIds();
            var activeUsersLoggedIn = activeUsers.Where(i => dictAccountsByUserId.ContainsKey(i))
                .Select(i => dictAccountsByUserId[i])
                .ToArray();

            return Ok(new { All = activeUsers.Count, LoggedIn = activeUsersLoggedIn.Length, activeUsersLoggedIn });
        }

        // GET api/admin/purge
        [HttpGet("purge")]
        [ProducesResponseType(200)]
        public ActionResult<string> Purge([FromQuery] string password, [FromQuery] int seconds)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            activeUserCounter.PurgeData(false, seconds);

            return Ok();
        }

        // GET api/admin/removedecks
        [HttpGet("removedecks")]
        [ProducesResponseType(200)]
        public ActionResult<string> RemoveDecks([FromQuery] string scraperId, [FromQuery] string password)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            var decksBefore = deckManager.Get();
            var decksAfter = decksBefore.Where(i => i.ScraperTypeId != scraperId).ToArray();

            var test = decksBefore.Where(i => i.ScraperTypeId.Contains("aetherhub-st")).ToArray();

            var nbRemoved = decksBefore.Count - decksAfter.Count();
            if (nbRemoved > 0)
            {
                var jsonDecks = JsonConvert.SerializeObject(new ConfigRootDecks { decks = decksAfter });
                var path = Path.Combine(configApp.FolderData, "decks.json");
                System.IO.File.WriteAllText(path, jsonDecks);
                deckManager.ReloadDecks();
            }

            return Ok(nbRemoved);
        }

        // GET api/admin/downloaddecks
        [HttpGet("downloaddecks")]
        [ProducesResponseType(200)]
        public ActionResult<string> DownloadDecks([FromQuery] string password, [FromQuery] bool reset = false)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            var lst = new List<DeckScraperResult>();

            //            Action<ScraperType> DownloadStreamDeckerAndMtgGoldfish = (scraperType) =>
            //            {
            //                var res = decksDownloader.DownloadDecks(scraperType, true);

            //                lock (lockDownloadDecks)
            //                {
            //                    lst.Add(res);
            //                    //container.AddDecks(res.Decks);
            //                }
            //            };

            //            var scraperTypes = new ScraperType[] { new ScraperType("streamdecker"), new ScraperType("mtggoldfish") };
            //#if DEBUG
            //            foreach (var scraperType in scraperTypes)
            //                DownloadStreamDeckerAndMtgGoldfish(scraperType);
            //#else
            //            System.Threading.Tasks.Parallel.ForEach(scraperTypes, (scraperType) =>
            //                DownloadStreamDeckerAndMtgGoldfish(scraperType));
            //#endif

            //            var res2 = decksDownloader.DownloadDecks(new ScraperType("aetherhub-tier1"), true);
            //            lst.Add(res2);
            //            ///container.AddDecks(res2.Decks);

            if (reset)
                deckManager.Reset();

            Task.Factory.StartNew(() =>
            {
                lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtgtop8-deckstobeat")));
                lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-muchabrew")));
                lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-tournaments-standard")));
                lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-tournaments-historicbo3")));
                lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-meta-standard")));
                lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-singlescoop")));
                lst.Add(decksDownloader.DownloadDecks(new ScraperType("aetherhub")));

                Task.Factory.StartNew(() =>
                {
                    //lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-meta-arenastandard")));
                    lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-meta-historicbo3")));
                    lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-fishfiveo")));
                    //lst.Add(decksDownloader.DownloadDecks(new ScraperType("streamdecker")));

                    Task.Factory.StartNew(() =>
                    {
                        lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-againsttheodds")));
                        lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-instantdecktech")));
                        lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-budgetmagic")));
                        lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-goldfishgladiators")));

                        Task.Factory.StartNew(() =>
                        {
                            lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtgatool-meta-standard")));
                            lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtgatool-meta-arenastandard")));
                            lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtgatool-meta-historicbo1")));
                            lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtgatool-meta-historicbo3")));
                            lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-budgetarena")));
                            lst.Add(decksDownloader.DownloadDecks(new ScraperType("mtggoldfish-streamhighlights")));
                        });
                    });
                });
            });

            //container.AddDecks(lst.SelectMany(i => i.Decks).ToArray());
            //return Ok(lst);
            return Ok();
        }

        // GET api/admin/downloaddecks/xyz
        [HttpGet("downloaddecks/{id}")]
        [ProducesResponseType(200)]
        public ActionResult<string> DownloadDecksScraperType([FromRoute] string id, [FromQuery] string password)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            var validation = decksDownloader.Validate(id);
            if (validation.isValid == false)
            {
                return BadRequest(new ErrorResponse(validation.error));
            }

            var scraperType = new ScraperType(id);
            var res = decksDownloader.DownloadDecks(scraperType);
            //container.AddDecks(res.Decks);

            //if (scraperType.Name == "meta")
            //    container.ReloadMasterData(true);

            return Ok(new DecksDownloadedResponse(res));
        }

        //        // GET api/admin/downloadstreamdeckerondisk
        //        [HttpGet("downloadstreamdeckerondisk")]
        //        [ProducesResponseType(200)]
        //        public ActionResult<string> DownloadStreamdeckerOnDisk([FromRoute] string id, [FromQuery]string password)
        //        {
        //            if (password != PASSWORD)
        //                return BadRequest(new ErrorResponse("Unauthorized"));

        //            var streamdeckersOnDisk = Directory
        //                .GetDirectories(Path.Combine(configApp.FolderDataDecks, "automatic", "streamdecker"))
        //                .Select(i => new DirectoryInfo(i).Name)
        //                .ToArray();

        //            decksDownloader.Init(container.allCards, new Dictionary<(ScraperTypeEnum, ScraperTypeFormatEnum), ICollection<string>>
        //            { { (ScraperTypeEnum.Streamdecker, ScraperTypeFormatEnum.Unknown), streamdeckersOnDisk } });

        //            var res = new List<DeckScraperResult>();
        //            Action<string> download = (streamdecker) =>
        //            {
        //                var r = decksDownloader.DownloadDecks(new ScraperType(ScraperTypeEnum.Streamdecker, streamdecker));
        //                res.Add(r);
        //                //container.AddDecks(r.Decks);
        //            };

        //#if DEBUG
        //            foreach (var streamdecker in streamdeckersOnDisk)
        //                download(streamdecker);
        //#else
        //            System.Threading.Tasks.Parallel.ForEach(streamdeckersOnDisk, streamdecker =>
        //                download(streamdecker));
        //#endif

        //            return Ok(res.Select(i => new DecksDownloadedResponse(i)));
        //        }

        // GET api/admin/reload
        [HttpGet("reload")]
        [ProducesResponseType(200)]
        public ActionResult<string> Reload([FromQuery] string password)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            //var initResult = container.ReloadMasterData(true);
            allCardsCache.Reload();
            deckManager.ReloadDecks();
            cacheDraftRatings.Reload();
            //draftRatingsLoader.Init(dictAllCards.Values);

            filesHashManager.Init(configApp.FolderData);

            return Ok(new StatusResponse("OK"));
        }

        [HttpGet("reloadcalendar")]
        [ProducesResponseType(200)]
        public ActionResult<string> ReloadCalendar([FromQuery] string password)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            cacheCalendarImageBinder.Reload();

            return Ok(new StatusResponse("OK"));
        }

        // GET api/admin/reloadsupporters
        [HttpGet("reloadsupporters")]
        [ProducesResponseType(200)]
        public ActionResult<string> ReloadSupporters([FromQuery] string password)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            cacheMembersEmail.Reload();

            return Ok(new StatusResponse(string.Join(Environment.NewLine, cacheMembersEmail.Get())));
        }

        //// GET api/admin/users
        //[HttpGet("users")]
        //[ProducesResponseType(200)]
        //public ActionResult<string> GetUsers([FromQuery]string password, [FromQuery]bool active)
        //{
        //    if (password != PASSWORD)
        //        return BadRequest(new ErrorResponse("Unauthorized"));

        //    var data = container.GetUsers(active);

        //    return Ok(new AdminGetUsersResponse(data));
        //}

        // GET api/admin/messages
        [HttpGet("messages")]
        [ProducesResponseType(200)]
        public ActionResult<string> GetContactFormMessages([FromQuery] string password)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            var messages = messageWriter.GetMessages();

            return Ok(new ContactFormMessageListResponse(messages));
        }

        // GET api/admin/forceUser
        [HttpGet("forceuser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult<string> ForceUser([FromQuery] string password, [FromQuery] string id)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ErrorResponse($"Parameter {nameof(id)} not provided"));

            Response.Cookies.Delete("userId");
            Response.Cookies.Append("userId", id, new CookieOptions { Expires = DateTime.UtcNow.AddYears(2) });

            return Ok(new StatusResponse($"Cookie userId set to {id}"));
        }

        // GET api/admin/clearuser
        [HttpGet("clearuser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult<string> ClearUser([FromQuery] string password, [FromQuery] string id)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ErrorResponse($"Parameter {nameof(id)} not provided"));

            container.ClearUser(id);

            return Ok(new StatusResponse($"User {id} cleared"));
        }

        // GET api/admin/audit
        [HttpGet("audit")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult<string> GetUserAudit([FromQuery] string password, [FromQuery] string id)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ErrorResponse($"Parameter {nameof(id)} not provided"));

            var auditLogFilePath = Path.Combine(configApp.FolderLogs, "audit", $"{id}_audit.txt");

            if (System.IO.File.Exists(auditLogFilePath) == false)
                return BadRequest(new ErrorResponse("Audit file not found for id " + id));

            using (FileStream logFileStream = new FileStream(auditLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader logFileReader = new StreamReader(logFileStream))
                return Ok(logFileReader.ReadToEnd());
        }

        // GET api/admin/logerrors
        [HttpGet("logerrors")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult<string> GetLogErrorsForDate([FromQuery] string password, [FromQuery] string id)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ErrorResponse($"Parameter {nameof(id)} not provided"));

            var logFilePath = Path.Combine(configApp.FolderLogs, $"log-errors-{id}.txt");

            if (System.IO.File.Exists(logFilePath) == false)
                return BadRequest(new ErrorResponse("Log file not found for " + id));

            using (FileStream logFileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader logFileReader = new StreamReader(logFileStream))
                return Ok(logFileReader.ReadToEnd());
        }

        // GET api/admin/log
        [HttpGet("log")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult<string> GetLogForDate([FromQuery] string password, [FromQuery] string id)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ErrorResponse($"Parameter {nameof(id)} not provided"));

            var logFilePath = Path.Combine(configApp.FolderLogs, $"log-{id}.txt");

            if (System.IO.File.Exists(logFilePath) == false)
                return BadRequest(new ErrorResponse("Log file not found for " + id));

            using (FileStream logFileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader logFileReader = new StreamReader(logFileStream))
                return Ok(logFileReader.ReadToEnd());
        }

        // GET api/admin/test
        [HttpGet("test")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult<string> GetUsersSummary([FromQuery] string password)//, [FromQuery]string t)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            tester.Test();
            return Ok();
        }

        // GET api/admin/removeusers
        [HttpGet("removeusers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetUsersSummary([FromQuery] string password, [FromQuery] string id)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            var validUserIds = new HashSet<string>(dictAccounts.Values.Select(i => i.MtgaHelperUserId));

            var folders = Directory.GetDirectories(configApp.FolderDataConfigUsers, $"{id}*");
            var foldersToDelete = folders.Where(i => validUserIds.Contains(Path.GetFileName(i)) == false).ToArray();

            Log.Information("RemoveUsers '{id}': {foldersToDelete} / {folders} to delete", id, foldersToDelete.Length, folders.Length);

            await Task.Factory.StartNew(async () =>
            {
                foreach (var folderToDelete in foldersToDelete)
                {
                    Directory.Delete(folderToDelete, true);
                    await Task.Delay(20);
                }

                Log.Information("RemoveUsers Completed '{id}': {foldersToDelete} / {folders} deleted", id, foldersToDelete.Length, folders.Length);
            });

            return Ok();
        }

        // GET api/admin/fixjsonall
        [HttpGet("fixjsonall")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> FixJsonAll()
        {
            var filepaths = Directory.GetFiles(configApp.FolderDataConfigUsers, "*.json", SearchOption.AllDirectories);
            await massJsonFileFixer.FixAllAsync(filepaths);

            return Ok();
        }

        // GET api/admin/deleteuserconfigfilebefore/20200101
        [HttpGet("deleteuserconfigfilesbefore/{strDate}")]
        [ProducesResponseType(200)]
        public ActionResult<string> DeleteUserConfigFilesBefore([FromRoute] string strDate, string prefix, [FromQuery] string password)
        {
            if (password != PASSWORD)
                return BadRequest(new ErrorResponse("Unauthorized"));

            if (DateTime.TryParse(strDate, out DateTime dateDeleteBefore) == false)
            {
                return BadRequest(new ErrorResponse("Invalid date"));
            }

            if (string.IsNullOrWhiteSpace(prefix)) prefix = null;
            var nbDeleted = configUserCleaner.DeleteConfigFilesBefore(dateDeleteBefore, prefix);

            return Ok($"{nbDeleted} deleted");
        }

        //// GET api/admin/summary
        //[HttpGet("summary")]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(400)]
        //public ActionResult<string> GetUsersSummary([FromQuery]string password)//, [FromQuery]string t)
        //{
        //    if (password != PASSWORD)
        //        return BadRequest(new ErrorResponse("Unauthorized"));

        //    //if (string.IsNullOrWhiteSpace(t))
        //    //    return BadRequest(new ErrorResponse($"Parameter {nameof(t)} not provided"));

        //    //if (t != "h" && t != "d")
        //    //    return BadRequest(new ErrorResponse($"Invalid value for parameter {nameof(t)}"));

        //    //var details = readerUserHistoryInfo
        //    //    .Init(Mapper.Map<TimeframeEnum>(t))
        //    //    .Init(TimeframeEnum.Daily)
        //    //    .GetInfoForTimeframe();

        //    //var allData = readerUserHistoryInfo.GetInfoHistory(default(DateTime));

        //    //return Ok(new AdminGetUsersSummaryResponse(details, allData, util.TimestampCreatedUtc));

        //    var info = readerUserHistoryInfo.GetInfo();
        //}
    }
}