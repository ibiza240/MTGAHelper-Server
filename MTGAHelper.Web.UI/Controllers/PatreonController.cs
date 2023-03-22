using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

namespace MTGAHelper.Web.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatreonController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Test([FromBody]dynamic payload)
        {
            return Ok(payload);
        }

        [HttpPost("webhook")]
        public IActionResult PatreonEvent([FromBody]dynamic payload)
        {
            try
            {
                var str = JsonConvert.SerializeObject(payload);
                Log.Warning(str);
                return Ok(str);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Ok(ex.ToString());
            }
        }

        public partial class PatreonPayload
        {
            public Data Data { get; set; }
            public Included[] Included { get; set; }
        }

        public partial class Data
        {
            public Attributes Attributes { get; set; }
            public Guid Id { get; set; }
            public Relationships Relationships { get; set; }
            public string Type { get; set; }
        }

        public partial class Attributes
        {
            public long AmountCents { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public object DeclinedSince { get; set; }
            public bool PatronPaysFees { get; set; }
            public object PledgeCapCents { get; set; }
        }

        public partial class Relationships
        {
            public Address Address { get; set; }
            public Address Card { get; set; }
            public Creator Creator { get; set; }
            public Creator Patron { get; set; }
            public Creator Reward { get; set; }
        }

        public partial class Address
        {
            public object Data { get; set; }
        }

        public partial class Creator
        {
            public CreatorData Data { get; set; }
            public Links Links { get; set; }
        }

        public partial class CreatorData
        {
            public long Id { get; set; }
            public string Type { get; set; }
        }

        public partial class Links
        {
            public Uri Related { get; set; }
        }

        public partial class Included
        {
        }
    }
}