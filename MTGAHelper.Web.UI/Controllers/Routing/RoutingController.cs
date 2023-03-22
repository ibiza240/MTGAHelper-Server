using Microsoft.AspNetCore.Mvc;
using System;

namespace MTGAHelper.Web.UI.Controllers.Routing
{
    [Controller]
    public class RoutingController : ControllerBase
    {
        [Obsolete]
        [Route("profile/lands")]
        public IActionResult LandsOld()
        {
            return Redirect("/index.html?r=lands");
        }

        [Route("{pageName}")]
        public IActionResult Generic([FromRoute]string pageName)
        {
            return Redirect($"/index.html?r={pageName}");
        }

        [Route("{pageName:regex(^(?!External).*?$)}/{prm}")]
        public IActionResult GenericWithParam([FromRoute]string pageName, [FromRoute]string prm)
        {
            return Redirect($"/index.html?r={pageName}%2F{prm}");
        }

        //[Route("{pageName}/{prm}/{prm2}")]
        //public IActionResult GenericWithParam([FromRoute]string pageName, [FromRoute]string prm, [FromRoute]string prm2)
        //{
        //    return Redirect($"/index.html?r={pageName}%2F{prm}%2F{prm2}");
        //}
    }
}