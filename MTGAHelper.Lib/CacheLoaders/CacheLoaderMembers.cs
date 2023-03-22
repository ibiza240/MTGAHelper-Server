using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.Logging;

namespace MTGAHelper.Lib.CacheLoaders
{
    public class MemberCsvRow
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class CacheLoaderMembers : ICacheLoader<IReadOnlySet<string>>
    {
        readonly string folderData;
        public CacheLoaderMembers(IDataPath folderData)
        {
            this.folderData = folderData.FolderData;
        }

        public IReadOnlySet<string> LoadData()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HeaderValidated = null,
                MissingFieldFound = null,
            };

            var fileMembers = Path.Combine(folderData, "Members.csv");
            LogExt.LogReadFile(fileMembers);
            var content = File.ReadAllText(fileMembers);

            using (var reader = new CsvReader(new StreamReader(fileMembers), config))
            {
                var records = reader.GetRecords<MemberCsvRow>().ToArray();
                var members = new HashSet<string>(records.Select(i => i.Email.Normalize()), StringComparer.OrdinalIgnoreCase);

                return members;
            }
        }
    }
}
