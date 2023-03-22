using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MTGAHelper.Entity.Config.App;
using System;

namespace MTGAHelper.Server.Data.Files.UserHistory
{
    public class UserHistoryDatesAvailableFromFile
    {
        readonly string folderDataConfigUsers;
        readonly Regex regexNumbersOnly = new Regex(@"^[0-9]*$", RegexOptions.Compiled);

        public UserHistoryDatesAvailableFromFile(IConfigUsersPath config)
        {
            this.folderDataConfigUsers = config.FolderDataConfigUsers;
        }

        IEnumerable<string> GetDates(string userId)
        {
            if (folderDataConfigUsers == null)
                return Array.Empty<string>();

            var folderUser = Path.Combine(folderDataConfigUsers, userId);
            if (Directory.Exists(folderUser) == false)
                return Array.Empty<string>();

            var directories = Directory.EnumerateDirectories(folderUser)
                .Select(i => new DirectoryInfo(i).Name)
                .Where(i => regexNumbersOnly.IsMatch(i))
                .OrderBy(i => i);

            return directories;
        }

        public ICollection<string> GetDatesOrdered(string userId)
        {
            return GetDates(userId).OrderBy(i => i).ToArray();
        }

        public ICollection<string> GetDatesOrderedDesc(string userId)
        {
            return GetDates(userId).OrderByDescending(i => i).ToArray();
        }
    }
}
