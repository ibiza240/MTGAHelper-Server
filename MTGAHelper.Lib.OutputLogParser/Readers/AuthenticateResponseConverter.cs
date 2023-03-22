﻿using MTGAHelper.Lib.OutputLogParser.Models.UnityCrossThreadLogger.AuthenticateResponse;

namespace MTGAHelper.Lib.OutputLogParser.Readers
{
    public class AuthenticateResponseConverter : GenericConverter<AuthenticateResponseResult, AuthenticateResponseRaw>, IMessageReaderUnityCrossThreadLogger
    {
        public override string LogTextKey => "AuthenticateResponse";
    }
}