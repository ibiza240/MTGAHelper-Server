﻿using System.Collections.Generic;
using MTGAHelper.Entity.OutputLogParsing;

namespace MTGAHelper.Lib.OutputLogParser.Models.UnityCrossThreadLogger.GetPreconDecksV3
{
    public class GetPreconDecksV3Result : MtgaOutputLogPartResultBase<PayloadRaw<ICollection<Entity.OutputLogParsing.CourseDeckRaw>>>//, IMtgaOutputLogPartResult<ICollection<GetDeckListResultDeckRaw>>
    {
        //public override ReaderMtgaOutputLogPartTypeEnum ResultType => ReaderMtgaOutputLogPartTypeEnum.GetDeckList;

        //public ICollection<GetDeckListResultDeck> Decks { get; set; }

        //public GetCombinedRankInfoRaw Raw { get; set; }

        //public ICollection<GetDeckListResultDeckRaw> Raw { get; set; }
    }

    //public class GetDeckListResultDeck
    //{
    //    public ICollection<GetDeckListResultDeck_CardSkin> cardSkins { get; set; }
    //    public string id { get; set; }
    //    public string name { get; set; }
    //    public string description { get; set; }
    //    public string format { get; set; }
    //    public int deckTileId { get; set; }
    //    public ICollection<CardWithAmount> MainDeck { get; set; }
    //    public ICollection<CardWithAmount> Sideboard { get; set; }
    //    public dynamic cardBack { get; set; }
    //    public string lastUpdated { get; set; }

    //    public ConfigModelRawDeck ToRawDeck()
    //    {
    //        return new ConfigModelRawDeck
    //        {
    //            Name = name,
    //            CardsMain = MainDeck.ToDictionary(i => i.Card.grpId, i => i.Amount),
    //            CardsSideboard = Sideboard.ToDictionary(i => i.Card.grpId, i => i.Amount),
    //        };
    //    }
    //}
}