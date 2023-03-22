using System.Collections.Generic;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Scryfall
{
    public record ScryfallModelRootObjectExtended : ScryfallModelRootObject
    {
        public void UpdateCardWithInfo(CardWriteable c, List<ScryfallModelRootObjectExtended> scryfallCards, ImageUris? imageUrisProvided = null)
        {
            if (imageUrisProvided == null)
                imageUrisProvided = image_uris;

            if (imageUrisProvided == null)
                imageUrisProvided = card_faces[0].image_uris;

            //if (c.SetArena.StartsWith("AN"))
            //    System.Diagnostics.Debugger.Break();

            c.IdScryfall = id;
            c.TypeLine = type_line;
            c.Layout = layout;
            c.ManaCost = mana_cost;
            c.OracleText = oracle_text;
            c.SetScryfall =
                c.SetArena.StartsWith("AN") && c.SetArena.Length == 3 ? c.SetArena.ToLower() :
                c.IsToken ? set.Substring(1) : set;
            c.ImageCardUrl = imageUrisProvided.Value.border_crop;
            c.ImageArtUrl = imageUrisProvided.Value.art_crop;
            c.Cmc = (int)cmc;

            //var cardExtendedArt = scryfallCards.FirstOrDefault(i => i.name == c.Name && i.set == c.SetScryfall && IsExtendedArt(i.frame_effects, i.promo));
            //if (cardExtendedArt != null)
            //{
            //    c.ImageCardExtendedArtUrl = cardExtendedArt.image_uris.border_crop;
            //}

            // BAD AND TEMP!!!!144
            if (c.SetArena == "J21" ||
                c.SetArena == "MH1" ||
                c.SetArena == "MH2")
                c.SetScryfall = "j21";
        }

        public bool IsExtendedArt(ICollection<string> frameEffects, bool promo)
        {
            return
                (
                    frameEffects != null &&
                    (
                        frameEffects.Contains("extendedart") ||
                        frameEffects.Contains("showcase")
                    )
                ) || promo;
        }
    }
}