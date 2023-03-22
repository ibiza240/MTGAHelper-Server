﻿using System;
using MTGAHelper.Entity;
using MTGAHelper.Entity.MtgaOutputLog;
using Newtonsoft.Json;

namespace MTGAHelper.Entity.GameEvents
{
    public class PlayLand : GameEventBase
    {
        public PlayLand(DateTime localTime, PlayerEnum player, Card card)
        {
            AtLocalTime = localTime;
            Player = player;
            Card = card;
        }

        public override string AsText => $"{PlayStr()} {Card.Name}.";

        private string PlayStr()
        {
            return Player == PlayerEnum.Me ? "You play" : "Opponent plays";
        }
    }
}
