using Minmaxdev.Cache.Common.Service;
using Minmaxdev.Data.Persistence.Common.Service;
using MtgaDecksPro.Cards.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MtgaDecksPro.Cards.Data.Handling.Service
{
    public class DocumentPersisterSetsInfo : DocumentPersisterBase<SetsInfo>
    {
        private readonly ICacheHandler<SetsManual> cacheHandlerSetsManual;
        private readonly ICacheHandler<List<SetScryfall>> dataHandlerSetsScryfall;

        public DocumentPersisterSetsInfo(
            DocumentPersisterConfiguration<SetsInfo> configuration,
            ICacheHandler<SetsManual> dataHandlerSetsManual,
            ICacheHandler<List<SetScryfall>> dataHandlerSetsScryfall
            )
            : base(configuration)
        {
            this.cacheHandlerSetsManual = dataHandlerSetsManual;
            this.dataHandlerSetsScryfall = dataHandlerSetsScryfall;
        }

        public override async Task<SetsInfo> Load()
        {
            var setsManual = await cacheHandlerSetsManual.Get();

            var sets = (await dataHandlerSetsScryfall.Get())
                .Select(setScryfall =>
                {
                    return new Set
                    {
                        //MtgaId = i.MtgaId,
                        CodeArena = setScryfall.arena_code ?? (setScryfall.digital ? setScryfall.code : null),
                        CodeScryfall = setScryfall.code,
                        ReleaseDate = DateTime.TryParse(setScryfall.released_at, out DateTime dt) ? dt : null,
                        Name = setScryfall.name,
                        NbCards = setsManual.SetsInfo.FirstOrDefault(i => i.Set.Equals(setScryfall.code, StringComparison.InvariantCultureIgnoreCase))?.CardsPerSet ?? 0,
                        PromoCardNumber = setsManual.SetsInfo.FirstOrDefault(i => i.Set.Equals(setScryfall.code, StringComparison.InvariantCultureIgnoreCase))?.PromoCardNumber ?? "0",
                        PromoCardIsStyle = setsManual.SetsInfo.FirstOrDefault(i => i.Set.Equals(setScryfall.code, StringComparison.InvariantCultureIgnoreCase))?.PromoCardIsStyle ?? false
                    };
                })
                .OrderBy(i => i.ReleaseDate)
                .ToArray();

            ApplyManualFixes(sets);

            return new SetsInfo
            {
                SetsByCodeScryfall = sets.ToDictionary(i => i.CodeScryfall, i => i),
                SetsStandard = setsManual.SetsStandard,
                SetsHistoric = setsManual.SetsHistoric,
            };
        }

        private void ApplyManualFixes(Set[] sets)
        {
            var setMIR = sets.FirstOrDefault(i => i.CodeArena == "MI");
            if (setMIR.Equals(default(Set)) == false)
                setMIR.CodeArena = "MIR";

            var setPLS = sets.FirstOrDefault(i => i.CodeArena == "PS");
            if (setPLS.Equals(default(Set)) == false)
                setPLS.CodeArena = "PLS";

            var setANA = sets.FirstOrDefault(i => i.CodeArena == "ANA");
            if (setANA.Equals(default(Set)) == false)
                setANA.NbCards = 31;

            var setANB = sets.FirstOrDefault(i => i.CodeArena == "ANB");
            if (setANB.Equals(default(Set)) == false)
            {
                setANB.CodeScryfall = "anb";
                setANB.NbCards = 116;
            }
        }
    }
}