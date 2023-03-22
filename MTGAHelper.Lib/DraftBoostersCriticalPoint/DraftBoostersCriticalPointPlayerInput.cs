namespace MTGAHelper.Lib.DraftBoostersCriticalPoint
{
    public class DraftBoostersCriticalPointPlayerInput
    {
        /// <summary>Total number of Rares of the set already in collection</summary>
        public int NbRares { get; set; }

        /// <summary>Total number of Mythic rares of the set already in collection</summary>
        public int NbMythics { get; set; }

        /// <summary>Total number of reward packs of the set already in collection</summary>
        public int NbPacks { get; set; }

        /// <summary>Amount of steps (0 - 29) taken on wildcard track.
        /// gold wildcard reward on reaching positions 6, 12, 18, 24;
        /// 30th step awards mythic wildcard</summary>
        public int WcTrackPosition { get; set; }
    }
}