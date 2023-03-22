using MTGAHelper.Lib.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MTGAHelper.Entity.Config.App;

namespace MTGAHelper.Lib.WebTesterConcurrent
{
    public class Tester
    {
        readonly IDataPath configPath;
        //IConfigManagerUsers configUsers;

        public Tester(IDataPath configPath)//, IConfigManagerUsers configUsers)
        {
            this.configPath = configPath;
            //this.configUsers = configUsers;
        }

        public void Test()
        {
            //var nbToSkip = new Random().Next(0, configUsers.Values.Count - 100);
            //var usersToLoad = configUsers.Values.Skip(nbToSkip).Take(50).Select(i => i.Id).ToArray();

            var rnd = new Random();
            var usersToLoad = Directory.GetDirectories(Path.Combine(configPath.FolderData, "configusers"))
                .Select(i => new DirectoryInfo(i).Name)
                .OrderBy(i => rnd.Next())
                .Take(50);

            //Parallel.ForEach(usersToLoad, (i) => Load(i));
            ////foreach (var i in usersToLoad) Load(i);

            var newIds = new List<string>();
            for (int i = 0; i < 50; i++)
                newIds.Add(Guid.NewGuid().ToString().Replace("-", ""));

            //Parallel.ForEach(newIds, (i) => LoadNew(i));



            // New + Existing, randomized
            Parallel.ForEach(newIds.Union(usersToLoad).OrderBy(i => rnd.Next()), (i) => Load(i));
        }

        private void LoadNew(string id)
        {
            string strResponse = null;
            var url = "https://localhost:5001";

            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = httpClient.SendAsync(request).Result;
                strResponse = response.Content.ReadAsStringAsync().Result;
            }

        }

        private void Load(string id)
        {
            string strResponse = null;
            var url = "https://localhost:5001/api";

            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url + "/user/collection");
                request.Headers.Add("userId", id);
                var response = httpClient.SendAsync(request).Result;
                strResponse = response.Content.ReadAsStringAsync().Result;

                request = new HttpRequestMessage(HttpMethod.Get, url + "/user/decks");
                request.Headers.Add("userId", id);
                response = httpClient.SendAsync(request).Result;
                strResponse = response.Content.ReadAsStringAsync().Result;

                request = new HttpRequestMessage(HttpMethod.Get, url + "/dashboard");
                request.Headers.Add("userId", id);
                response = httpClient.SendAsync(request).Result;
                strResponse = response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
