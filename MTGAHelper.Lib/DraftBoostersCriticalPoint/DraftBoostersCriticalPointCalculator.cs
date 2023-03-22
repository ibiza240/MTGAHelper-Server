//using MathNet.Numerics.Distributions;

using MTGAHelper.Entity;
using System;

namespace MTGAHelper.Lib.DraftBoostersCriticalPoint
{
    public class DraftBoostersCriticalPointCalculator
    {
        const float raresPerPack = 7f / 8 * 11 / 12;
        const float mythicsPerPack = 1f / 8 * 11 / 12;

        public DraftBoostersCriticalPointResult Calculate(
            DraftBoostersCriticalPointPlayerInput input,
            DraftBoostersCriticalPointAssumptions assum)
        {
            var raresPerSet = assum.NbUniqueRaresInSet * 4;
            var mythicsPerSet = assum.NbUniqueMythicsInSet * 4;

            var r = new DraftBoostersCriticalPointResult
            {
                InfoRare = new DraftBoostersCriticalPointRarityInfo
                {
                    T = raresPerSet,
                    R = input.NbRares,
                    P = input.NbPacks,
                    N = assum.NbRaresPerDraft,
                    W = assum.NbRewardPacksPerDraft,
                },
                InfoMythic = new DraftBoostersCriticalPointRarityInfo
                {
                    T = mythicsPerSet,
                    R = input.NbMythics,
                    P = input.NbPacks,
                    N = assum.NbMythicsPerDraft,
                    W = assum.NbRewardPacksPerDraft,
                },
                NbRaresMissing = raresPerSet - input.NbRares,
                NbMythicsMissing = mythicsPerSet - input.NbMythics
            };

            var expectedMissingRares = r.NbRaresMissing - input.NbPacks * raresPerPack;
            var expectedMissingMythics = r.NbMythicsMissing - input.NbPacks * mythicsPerPack;

            r.ExpectedNbDraftsToPlaysetRares = Math.Max(0, expectedMissingRares / (assum.NbRaresPerDraft + assum.NbRewardPacksPerDraft * raresPerPack));
            r.ExpectedNbDraftsToPlaysetMythics = Math.Max(0, expectedMissingMythics / (assum.NbMythicsPerDraft + assum.NbRewardPacksPerDraft * mythicsPerPack));

            //r.ChanceFullPlaysetRares = CalculateChanceFullPlayset(raresPerPack, input.NbPacksCollected, r.NbRaresMissing);
            //r.ChanceFullPlaysetMythics = CalculateChanceFullPlayset(mythicsPerPack, input.NbPacksCollected, r.NbMythicsMissing);

            // going on data from https://mtgazone.com/wildcards/#Booster_Packs_Substitution but still not sure how to interpret those tables
            r.ExpectedRareWcsFromPacks = input.NbPacks / 24f;

            // assume input.WcTrackPosition is an int between 0 and 29 with a gold wildcard on positions 6, 12, 18, 24
            // we first find out the rare wildcards that amount to less than one full (30 pack) cycle
            // and then add the 30 pack cycles, 4 rares per full cycle 
            r.RareWcsFromTrack = ExtraWcTrackPosition(input) + input.NbPacks / 30 * 4;
            r.MythicWcsFromTrack = (input.NbPacks + input.WcTrackPosition) / 30;

            return r;
        }

        private static int ExtraWcTrackPosition(DraftBoostersCriticalPointPlayerInput input)
        {
            int extraWcTrackPosition = (input.NbPacks % 30 + input.WcTrackPosition) / 6; // integer division!

            if (extraWcTrackPosition > 4)
                extraWcTrackPosition -= 1; // upgraded to mythic wc

            extraWcTrackPosition -= input.WcTrackPosition / 6; // these were already rewarded
            return extraWcTrackPosition;
        }

        //private double CalculateChanceFullPlayset(double chancePerPack, int packsAvailable, int nbMissing)
        //{
        //    if (nbMissing > packsAvailable)
        //        return 0;

        //    if (nbMissing <= 0)
        //        return 1;

        //    return 1 - Binomial.CDF(chancePerPack, packsAvailable, nbMissing - 1);
        //}
    }
}
