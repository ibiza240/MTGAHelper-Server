using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Lib.Config;
using Newtonsoft.Json;

namespace MTGAHelper.Lib.Matches
{
    public class MatchesReader
    {
        readonly IConfigUsersPath configPath;

        public MatchesReader(IConfigUsersPath configPath)
        {
            this.configPath = configPath;
        }

        public ICollection<MatchResult> ReadMatches(string userId)
        {
            var filesMatches = Directory.GetFiles(Path.Combine(configPath.FolderDataConfigUsers, userId), "*Matches*.json", SearchOption.AllDirectories);
            var matchesRaw = filesMatches
                .Select(i => JsonConvert.DeserializeObject<InfoByDate<ICollection<MatchResult>>>(File.ReadAllText(i)))
                .ToArray();

            //if (matchesRaw == null) System.Diagnostics.Debugger.Break();

            var matches = matchesRaw
                .SelectMany(i => i.Info)
                .ToArray();

            return matches;
        }
    }
}
