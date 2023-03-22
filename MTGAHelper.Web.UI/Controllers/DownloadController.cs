using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MTGAHelper.Entity;
using MTGAHelper.Lib.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MTGAHelper.Entity.Config.App;

namespace MTGAHelper.Web.UI.Controllers
{
    [Route("api/[controller]")]
    public class DownloadController : Controller
    {
        readonly string folderData;

        public DownloadController(IDataPath configPath)
        {
            folderData = configPath.FolderData;
        }

        //GET api/download/draftRatings
        [HttpGet("{datatype}")]
        public IActionResult DownloadData(string datatype)
        {
            if (Enum.TryParse(datatype, out DataFileTypeEnum _) == false)
                return NotFound();

            return File(Path.Combine(folderData.Replace("..", "~"), datatype + ".json"), "application/json", datatype + ".json");
        }

    }
}
