using Microsoft.AspNetCore.Mvc;
using MTGAHelper.Lib;
using MTGAHelper.Lib.Config.Decks;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Web.Models.Response;
using MTGAHelper.Web.Models.Response.Meta;
using System.Collections.Generic;

namespace MTGAHelper.Web.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaController : MtgaHelperControllerBase
    {
        private readonly ConfigManagerDecks deckManager;

        public MetaController(
            CacheSingleton<IReadOnlySet<string>> cacheMembers,
            ISessionContainer container,
            IConfigManagerUsers configUsers,
            ConfigManagerDecks managerDecks
            )
            : base(cacheMembers, container, configUsers)
        {
            this.deckManager = managerDecks;
        }

        // GET api/meta
        [HttpGet]
        [ProducesResponseType(200)]
        public ActionResult<IResponse> Get()
        {
            return Ok(new MetaResponse(deckManager.Get()));
        }
    }
}