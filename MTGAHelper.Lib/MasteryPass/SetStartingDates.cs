using System;
using System.Collections.Generic;

namespace MTGAHelper.Lib.MasteryPass
{
    public static class SetStartingDates
    {
        public static Dictionary<string, DateTime> DictStartingDate => new Dictionary<string, DateTime>
        {
            ["ELD"] = new DateTime(2019, 9, 26, 10, 0, 0, DateTimeKind.Utc),
            ["THB"] = new DateTime(2020, 1, 16, 10, 0, 0, DateTimeKind.Utc),
            ["IKO"] = new DateTime(2020, 4, 16, 10, 0, 0, DateTimeKind.Utc),
            ["M21"] = new DateTime(2020, 6, 25, 10, 0, 0, DateTimeKind.Utc),
            ["ZNR"] = new DateTime(2020, 9, 17, 11, 0, 0, DateTimeKind.Utc),
            ["KHM"] = new DateTime(2021, 1, 28, 11, 0, 0, DateTimeKind.Utc),
            ["STX"] = new DateTime(2021, 4, 15, 11, 0, 0, DateTimeKind.Utc),
            ["AFR"] = new DateTime(2021, 7, 8, 11, 0, 0, DateTimeKind.Utc),
            ["MID"] = new DateTime(2021, 9, 16, 11, 0, 0, DateTimeKind.Utc),
            ["VOW"] = new DateTime(2021, 11, 11, 11, 0, 0, DateTimeKind.Utc),
            ["NEO"] = new DateTime(2022, 2, 10, 11, 0, 0, DateTimeKind.Utc),
            ["SNC"] = new DateTime(2022, 4, 28, 11, 0, 0, DateTimeKind.Utc),
            ["HBG"] = new DateTime(2022, 7, 7, 11, 0, 0, DateTimeKind.Utc),
            ["DMU"] = new DateTime(2022, 9, 1, 11, 0, 0, DateTimeKind.Utc),
            ["BRO"] = new DateTime(2022, 11, 15, 11, 0, 0, DateTimeKind.Utc),
            ["ONE"] = new DateTime(2023, 2, 7, 11, 0, 0, DateTimeKind.Utc),
            ["MOM"] = new DateTime(2023, 4, 15, 11, 0, 0, DateTimeKind.Utc),
        };
    }
}