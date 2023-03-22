using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using MTGAHelper.Entity;
using MTGAHelper.Lib;
using Newtonsoft.Json;

namespace MTGAHelper.Tools.CosmosDB.Downloader
{
    public class SupportersProvider
    {
        public class MemberCsvRow
        {
            public string Name { get; set; }
            public string Email { get; set; }
        }

        private string folderAccounts;

        public ICollection<string> GetSupportersUserIds(string filepathMembers, string folderAccounts)
        {
            this.folderAccounts = folderAccounts;

            var emails = GetEmailsFromMembersFile(filepathMembers);
            var userIds = emails
                .Select(i => GetUserIdFromEmail(i))
                .Where(i => i != null)
                .ToArray();
            return userIds;
        }

        ICollection<string> GetEmailsFromMembersFile(string filepathMembers)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HeaderValidated = null,
                MissingFieldFound = null,
            };

            using (var reader = new CsvReader(new StreamReader(filepathMembers), config))
            {
                var records = reader.GetRecords<MemberCsvRow>().ToArray();
                var members = new HashSet<string>(records.Select(i => i.Email.Normalize()), StringComparer.OrdinalIgnoreCase);

                return members;
            }
        }

        public string GetUserIdFromEmail(string email)
        {
            var emailHash = EmailToHash(email);
            var account = OpenAccountFile(emailHash);
            return account?.MtgaHelperUserId;
        }

        private AccountModel OpenAccountFile(string emailHash)
        {
            var filepath = Path.Combine(folderAccounts, $"{emailHash}.json");

            if (File.Exists(filepath) == false)
                return null;

            var fileContent = File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<AccountModel>(fileContent);
        }

        public string EmailToHash(string email)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(email.ToLower());
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                var emailHash = hashBytes.ToBase32String(false);
                return emailHash;
            }
        }
    }
}
