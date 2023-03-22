using AutoMapper;
using HtmlAgilityPack;
using MTGAHelper.Entity;
using MTGAHelper.Lib.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace MTGAHelper.Lib.Scraping.CalendarScraper
{
    public class CalendarScraperMtgaAssistant
    {
        private readonly IMapper mapper;

        public CalendarScraperMtgaAssistant(
            IMapper mapper
            )
        {
            this.mapper = mapper;
        }

        public async Task<ICollection<ConfigModelCalendarItem>> GetCalendar()
        {
            var result = await GetMTGACalendarItems();
            var mapped = mapper.Map<ICollection<ConfigModelCalendarItem>>(result);

            foreach (var m in mapped)
            {
                var match = Regex.Match(m.DateRange, @"^(.*?)(\d+\s*-\s*\d+)$");
                if (match.Success)
                {
                    var month = match.Groups[1].Value.Trim();
                    var days = match.Groups[2].Value.Split('-').Select(i => i.Trim()).ToArray();
                    m.DateRange = $"{month} {days[0]} to {month} {days[1]}";
                }
                else
                {
                    m.DateRange = m.DateRange.Replace("-", " to ");
                }
            }

            return mapped;
        }

        private async Task<ICollection<CalendarScraperMtgaAssistantItemModel>> GetMTGACalendarItems()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("ApiKey", "Aetherhub_H3f#zs2");
                var response = await client.GetStringAsync("https://aetherhub.com/Api/MTGAFrontpage/");

                var fixBugFromProvider = FixBugFromProvider(response);

                var frontpage = JsonConvert.DeserializeObject<CalendarScraperMtgaAssistantMTGAFrontpage>(fixBugFromProvider);
                var calendar = JsonConvert.DeserializeObject<ICollection<CalendarScraperMtgaAssistantItemModel>>(frontpage.Model.Calendar);
                return calendar;
            }
        }

        private string FixBugFromProvider(string providerResponse)
        {
            return providerResponse;
        }
        //private string FixBugFromProvider(string providerResponse)
        //{
        //    // Replace literal \" found in the response by just "
        //    //var result = providerResponse.Replace("\\\"", "\"");
        //    var result = providerResponse.ToString();

        //    var pattern = @"\{ \""([^:]+)"",";
        //    var regex = new Regex(pattern);
        //    var replacement = @"{ ""title"": ""\1"",";
        //    result = regex.Replace(result, replacement);

        //    return result;
        //}
    }
}
