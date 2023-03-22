using MTGAHelper.Lib.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MTGAHelper.Lib.UserStats
{
    public class UserHistoryInfo
    {
        public string UserId { get; set; }
        //public string Filepath { get; set; }
        public DateTime TimestampUtc { get; set; }
        public UserHistoryInfoTypeEnum Type { get; set; }
    }
}
