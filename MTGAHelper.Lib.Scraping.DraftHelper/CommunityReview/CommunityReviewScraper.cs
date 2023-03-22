using MTGAHelper.Entity;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace MTGAHelper.Lib.Scraping.DraftHelper.CommunityReview
{
    public class CommunityReviewScraper
    {
        private readonly Dictionary<string, string> sets = new Dictionary<string, string>
        {
            { "THB","https://docs.google.com/spreadsheets/d/1zxR9zQxdEH9XI7CZmxiKeTQdU2nD8bVgYJ4bLUwV53s/gviz/tq?headers=2&range=A1%3AZ&sheet=Table%20Source&tqx=reqId%3A0;responseHandler%3A__JsonpRequest_cb__.getResponse0_" },
            { "ELD","https://docs.google.com/spreadsheets/d/1B-bEUGANnGFPB4zW-vAV8zHjUZINwLU8Qq1sVlgIdpU/gviz/tq?headers=2&range=A1%3AZ&sheet=Table%20Source&tqx=reqId%3A0;responseHandler%3A__JsonpRequest_cb__.getResponse0_" },
            { "M20","https://docs.google.com/spreadsheets/d/1BAPtQv4U9KUAtVzkccJlPS8cb0s_uOcGEDORip5uaQg/gviz/tq?headers=2&range=A1%3AZ&sheet=Table%20Source&tqx=reqId%3A0;responseHandler%3A__JsonpRequest_cb__.getResponse0_" },
            { "WAR","https://docs.google.com/spreadsheets/d/1pk3a1YKGas-NI4ze_8hbwOtVRdYAbzCDIBS9MKjcQ7M/gviz/tq?headers=2&range=A1%3AZ&sheet=Table%20Source&tqx=reqId%3A0;responseHandler%3A__JsonpRequest_cb__.getResponse0_" },
            { "RNA","https://docs.google.com/spreadsheets/d/1DfcITmtWaBHtiDYLYWHzizw-AOrB3GUQaapc_BqfeH4/gviz/tq?headers=2&range=A1%3AZ&sheet=Table%20Source&tqx=reqId%3A0;responseHandler%3A__JsonpRequest_cb__.getResponse0_" },
            { "GRN","https://docs.google.com/spreadsheets/d/1FPN3hgl6x_ePq-8On7Ebr8L6WHSU2IznoWSBoGaC_RQ/gviz/tq?headers=2&range=A1%3AZ&sheet=Table%20Source&tqx=reqId%3A0;responseHandler%3A__JsonpRequest_cb__.getResponse0_" },
            { "M19","https://docs.google.com/spreadsheets/d/1aZlqE-8mGdfQ50NXUaP-9dRk3w_hp9XmcBqZ_4x3_jk/gviz/tq?headers=2&range=A1%3AZ&sheet=TableSource&tqx=reqId%3A0;responseHandler%3A__JsonpRequest_cb__.getResponse0_" },
            { "DOM","https://docs.google.com/spreadsheets/d/1cc-AOmpQZ7vKqxDTSSvhmRBVOCy_569kT0S-j-Rpbj8/gviz/tq?headers=2&range=A1%3AZ&sheet=Table%20Source&tqx=reqId%3A0;responseHandler%3A__JsonpRequest_cb__.getResponse0_" },
            { "RIX","https://docs.google.com/spreadsheets/d/1CNg-FDp-pOtQ14Qj-rIBO-yfyr5YcPA6n6ztrEe4ATg/gviz/tq?headers=2&range=A1%3AZ&sheet=TableSource&tqx=reqId%3A0;responseHandler%3A__JsonpRequest_cb__.getResponse0_" },
            { "XLN","https://docs.google.com/spreadsheets/d/1KDtLJd6Nkrv_DDpFs84soBZcWPG1tg79TnVEh-enPz8/gviz/tq?headers=2&range=A1%3AZ&sheet=TableSource&tqx=reqId%3A0;responseHandler%3A__JsonpRequest_cb__.getResponse0_" }
        };

        public class UrlToScrapeModel
        {

            public string UrlPartSet { get; set; }


            public UrlToScrapeModel(string urlPart)
            {
                UrlPartSet = urlPart;
            }
        }

        private readonly SharedTools sharedTools;

        public CommunityReviewScraper(SharedTools sharedTools)
        {
            this.sharedTools = sharedTools;
        }

        public DraftRatings Scrape(string setFilter = "")
        {
            var ret = new DraftRatings();

            foreach (var set in sets.Keys.Where(i => setFilter == "" || i == setFilter))
            {
                var ratings = GetRatingsForSet(set);
                ret.RatingsBySet.Add(set, ratings);
            }

            return ret;
        }

        public DraftRatingScraperResultForSet GetRatingsForSet(string set)
        {
            var ret = new DraftRatingScraperResultForSet();
            var cardList = new GVizModel();
            var urlFormatted = sets[set];
            HttpClient client = new HttpClient();
            var response = client.GetAsync(urlFormatted).Result;
            if (response.IsSuccessStatusCode)
            {
                string jsonData = response.Content.ReadAsStringAsync().Result;
                jsonData = jsonData.Trim();
                jsonData = jsonData.Substring(jsonData.IndexOf("{"));  //this comes through as a javascript assignment.  remove anything prior to the start of the json array

                if (jsonData.EndsWith(");"))  //get rid of the ending javascript ; also
                {
                    jsonData = jsonData.Remove(jsonData.Length - 2);
                }
                cardList = JsonConvert.DeserializeObject<GVizModel>(jsonData);
                //cardname = CardList.table.rows[x].c[1].v
                //cardcolor = CardList.table.rows[x].c[2].v
                //rarity = CardList.table.rows[x].c[3].v
                //rating = CardList.table.rows[x].c[5].v

                var ratings = cardList.table.rows.Select(i => {

                    if (i.c.Length < 6) Debugger.Break ();

                    return new DraftRating
                    {
                        CardName = i.c[1].v.ToString().Trim(),
                        Description = "",
                        RatingValue = i.c[5].v,
                        RatingToDisplay = i.c[5].v.ToString("0.00")
                    };
                }).ToArray();

                ret.Ratings = ratings;
                ret.TopCommonCardsByColor = sharedTools.GetTop5CommonByColor(ratings);
            }
            return ret;
        }
    }
}
