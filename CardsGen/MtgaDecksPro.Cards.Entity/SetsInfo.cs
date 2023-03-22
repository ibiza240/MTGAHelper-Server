using System.Collections.Generic;
using System.Linq;

namespace MtgaDecksPro.Cards.Entity
{
    public record SetsInfo
    {
        public Dictionary<string, Set> SetsByCodeScryfall { get; init; } = new Dictionary<string, Set>();
        public IEnumerable<string> SetsStandard { get; init; } = Enumerable.Empty<string>();
        public IEnumerable<string> SetsHistoric { get; init; } = Enumerable.Empty<string>();

        public ICollection<string> SetsStandardArena => SetsStandard
            .Union(new[] { "ana", "anb" })
            .ToArray();

        public ICollection<string> SetsHistoricAll => SetsStandardArena
            .Union(SetsHistoric)
            .ToArray();

        public ICollection<string> SetsAlchemy => SetsStandardArena.Union(
                SetsByCodeScryfall.Values
                    .Where(i => i.Name.Contains("Alchemy"))
                    .Select(i => i.CodeScryfall)
            )
            .ToArray();
    }
}