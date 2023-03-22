﻿using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Lib.OutputLogParser.Models.UnityCrossThreadLogger.GetPreconDecksV3;
using System.Collections.Generic;

namespace MTGAHelper.Lib.OutputLogParser.Readers.UnityCrossThreadLogger
{
    //public class GetDecksListV3Converter : IReaderMtgaOutputLogJson
    //{
    //    DeckListConverter deckConverter;

    //    public GetDecksListV3Converter(DeckListConverter deckConverter)
    //    {
    //        this.deckConverter = deckConverter;
    //    }

    //    public IMtgaOutputLogPartResult ParseJson(string json)
    //    {
    //        //try
    //        //{
    //            var decksRaw = JsonConvert.DeserializeObject<IGetDeckListResultDeckRawCollection>(json);

    //            var result = new GetDecksListResult
    //            {
    //                //Decks = decksRaw.Select(i =>
    //                //{
    //                //    var d = Mapper.Map<GetDeckListResultDeck>(i);
    //                //    d.MainDeck = deckConverter.ConvertCards(i.mainDeck);
    //                //    d.Sideboard = deckConverter.ConvertCards(i.sideboard);
    //                //    return d;
    //                //}).ToArray()
    //                Raw = decksRaw
    //            };

    //            return result;
    //        //}
    //        //catch (Exception ex)
    //        //{
    //        //    System.Diagnostics.Debugger.Break();
    //        //    return null;
    //        //}
    //    }
    //}
    public class GetPreconDecksV3Converter : GenericConverter<GetPreconDecksV3Result, PayloadRaw<ICollection<Entity.OutputLogParsing.CourseDeckRaw>>>, IMessageReaderUnityCrossThreadLogger
    {
        public override string LogTextKey => "<== Deck.GetPreconDecks";
    }
}