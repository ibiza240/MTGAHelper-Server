using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Entity.CollectionDecksCompare;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using MTGAHelper.Lib.CollectionDecksCompare;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Users;
using System.Linq;
using MTGAHelper.Entity.Config.Decks;
using System;

namespace MTGAHelper.UnitTests
{
    [TestClass]
    public class TestsDashboardDetails : TestsBase
    {
        [TestMethod]
        public void T()
        {
            var service = provider.GetRequiredService<ICardsMissingComparer>();

            var config = new ConfigModelUser
            {
                isDebug = true,
                Weights = new Dictionary<RarityEnum, UserWeightDto>
                {
                    [RarityEnum.Mythic] = new UserWeightDto(10000f, 5000f),
                    [RarityEnum.RareLand] = new UserWeightDto(1000f, 500f),
                    [RarityEnum.RareNonLand] = new UserWeightDto(1000f, 500f),
                    [RarityEnum.Uncommon] = new UserWeightDto(100f, 50f),
                    [RarityEnum.Common] = new UserWeightDto(10f, 5f),
                }
            };

            var cards = allCards.Values
                .Where(i => i.Type.Contains("Legendary Artifact"))
                .GroupBy(i => i.Name)
                .Take(1)
                .Select((i, j) => new CardWithAmount(i.First(), j))
                //// Debug
                //.Skip(3).Take(1)
                .ToArray();

            var decks = new[] { 1, 2, 3, 4 }
                .SelectMany(x =>
                    cards.SelectMany(i => new Deck[]
                    {
                        new Deck(i.Card.Name +  " Main " + i.Amount, null, new []
                        {
                            new DeckCard(i.Card, new Random().Next(1, 5), DeckCardZoneEnum.Deck)
                        }),
                        new Deck(i.Card.Name +  " Sideboard " + i.Amount, null, new []
                        {
                            new DeckCard(i.Card, new Random().Next(1, 5), DeckCardZoneEnum.Sideboard)
                        }),
                    })
                )
                .ToArray();

            var result = service.Init(config, cards).Compare(decks, Array.Empty<IDeck>(), Array.Empty<IDeck>());
            var result2 = result.GetModelDetails();

            var data = result.ByCard
                .Select(i => $"{i.Key}\t{i.Value.MissingWeight}\t{i.Value.NbDecks}\t{i.Value.NbMissing}\t{i.Value.NbMissingMain}\t{i.Value.NbMissingSideboard}\t{i.Value.NbOwned}\t{i.Value.NbRequired}\t{i.Value.NbRequiredMain}\t{i.Value.NbRequiredSideboard}")
                ;

            var str =
                "Name\tMissingWeight\tNbDecks\tNbMissing\tNbMissingMain\tNbMissingSide\tNbOwned\tNbRequired\tNbRequiredMain\tNbRequiredSideboard" +
                Environment.NewLine +
                string.Join(Environment.NewLine, data);
        }
    }
}
