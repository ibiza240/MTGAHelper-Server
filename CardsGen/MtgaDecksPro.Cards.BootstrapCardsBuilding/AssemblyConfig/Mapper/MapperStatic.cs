using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Scryfall;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.AssemblyConfig.Mapper
{
    internal static class MapperStatic
    {
        public static bool ConvertIsPrimary(ScryfallModelRootObjectExtended source)
        {
            // No hint of card with multiple sides
            if (source.card_faces == null || source.card_faces.Count == 0)
                return true;

            return source.name == source.card_faces[0].name;
        }
    }
}