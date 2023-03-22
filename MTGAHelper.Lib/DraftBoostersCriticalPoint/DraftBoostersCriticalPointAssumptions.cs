namespace MTGAHelper.Lib.DraftBoostersCriticalPoint
{
    public class DraftBoostersCriticalPointAssumptions
    {
        public int NbUniqueRaresInSet { get; set; } = 53;
        public int NbUniqueMythicsInSet { get; set; } = 15;

        /// <summary>Number of "new" Rares pulled from a draft on average (Higher earlier, lesser later, but an average across the set is fine.)</summary>
        public float NbRaresPerDraft { get; set; } = 3.5f;

        /// <summary>Number of "new" Mythic rares pulled from a draft on average (Higher earlier, lesser later, but an average across the set is fine.)</summary>
        public float NbMythicsPerDraft { get; set; } = 0.5f;

        /// <summary>Average number of reward packs from doing the draft.</summary>
        public float NbRewardPacksPerDraft { get; set; } = 1.38f;
    }
}