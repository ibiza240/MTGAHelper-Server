﻿using System.Collections.Generic;

namespace MTGAHelper.Lib.OutputLogParser.Models.GRE.MatchToClient.DieRollResultsResp
{
    public class PlayerDieRoll
    {
        public int systemSeatId { get; set; }
        public int rollValue { get; set; }
    }

    public class DieRollResultsRespData
    {
        public List<PlayerDieRoll> playerDieRolls { get; set; }
    }

    public class DieRollResultsRespRaw
    {
        public string type { get; set; }
        public List<int> systemSeatIds { get; set; }
        public int msgId { get; set; }
        public DieRollResultsRespData dieRollResultsResp { get; set; }
    }
}
