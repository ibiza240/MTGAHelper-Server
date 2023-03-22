﻿using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Lib.OutputLogParser.Models.UnityCrossThreadLogger.GetPlayerQuests;
using System.Collections.Generic;

namespace MTGAHelper.Lib.OutputLogParser.Readers.UnityCrossThreadLogger
{
    public class GetPlayerQuestsConverter : GenericConverter<GetPlayerQuestsResult, GetPlayerQuestsRaw>, IMessageReaderUnityCrossThreadLogger
    {
        public override string LogTextKey => "<== Quest_GetQuests";
    }
}