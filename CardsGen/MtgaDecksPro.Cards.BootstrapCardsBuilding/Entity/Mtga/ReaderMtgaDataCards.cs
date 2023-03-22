using Minmaxdev.Data.Persistence.Common.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Mtga
{
    public class ReaderMtgaDataCards
    {
        private readonly IDocumentPersister<List<MtgaDataCardsRootObjectExtended>> fileLoaderCards;
        private readonly IDocumentPersister<List<MtgaDataLocRootObject>> fileLoaderLoc;

        public ReaderMtgaDataCards(
            IDocumentPersister<List<MtgaDataCardsRootObjectExtended>> fileLoaderCards,
            IDocumentPersister<List<MtgaDataLocRootObject>> fileLoaderLoc
            )
        {
            this.fileLoaderCards = fileLoaderCards;
            this.fileLoaderLoc = fileLoaderLoc;
        }

        public async Task<List<MtgaDataCardsRootObjectExtended>> GetMtgaCards()
        {
            var mtgaCards = (await fileLoaderCards.Load()).Where(i => i.TitleId != 0);
            var mtgaLoc = await fileLoaderLoc.Load();

            var dictLoc = mtgaLoc
                .First(i => i.isoCode == "en-US")
                .keys
                .ToDictionary(i => i.id, i => i.raw ?? i.text);

            foreach (var c in mtgaCards)
            {
                if (dictLoc.ContainsKey(c.TitleId))
                    c.Name = dictLoc[c.TitleId];
                else
                    //logger.Error($"Cannot find titleId {c.titleId} in loc file");
                    throw new Exception($"Cannot find titleId {c.TitleId} in loc file");

                //if (dictLoc.ContainsKey(c.FlavorId))
                //    c.Flavor = dictLoc[c.FlavorId];
                //else
                //    //logger.Error($"Cannot find flavorId {c.flavorId} in loc file");
                //    throw new Exception($"Cannot find flavorId {c.FlavorId} in loc file");
            }

            //var bogus = mtgaCards.Where(x =>
            //{
            //    try
            //    {
            //        var i = Convert.ToInt32(x.toughness.Replace("*", "0"));
            //        return false;
            //    }
            //    catch
            //    {
            //        return true;
            //    }
            //}).ToArray();

            return mtgaCards
                .Where(i => i.Name.StartsWith("A-") == false)
                .ToList();
        }
    }
}