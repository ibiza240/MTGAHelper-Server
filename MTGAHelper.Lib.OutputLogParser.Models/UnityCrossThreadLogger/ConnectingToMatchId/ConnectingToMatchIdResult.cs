﻿using MTGAHelper.Entity.OutputLogParsing;

namespace MTGAHelper.Lib.OutputLogParser.Models.UnityCrossThreadLogger.ConnectingToMatchId
{
    public class ConnectingToMatchIdResult : MtgaOutputLogPartResultBase<string>, ITagMatchResult//, IMtgaOutputLogPartResult<ConnectingToMatchIdRaw>
    {
    }

    //public class ConnectingToMatchIdRaw
    //{
    //    public string matchId { get; set; }
    //}
}