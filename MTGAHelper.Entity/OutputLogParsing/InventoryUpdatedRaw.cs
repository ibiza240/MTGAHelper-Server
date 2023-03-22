﻿using System;
using System.Collections.Generic;

namespace MTGAHelper.Entity.OutputLogParsing
{
    //    public class ArtSkinsAdded
    //    {
    //        public int artId { get; set; }
    //        public string ccv { get; set; }
    //    }

    //    public class Delta
    //    {
    //        public int gemsDelta { get; set; }
    //        public int goldDelta { get; set; }
    //        public List<object> boosterDelta { get; set; }
    //        public List<int> cardsAdded { get; set; }
    //        public List<object> decksAdded { get; set; }
    //        public List<object> starterDecksAdded { get; set; }
    //        public List<object> vanityItemsAdded { get; set; }
    //        public List<object> vanityItemsRemoved { get; set; }
    //        public int draftTokensDelta { get; set; }
    //        public int sealedTokensDelta { get; set; }
    //        public double vaultProgressDelta { get; set; }
    //        public int wcCommonDelta { get; set; }
    //        public int wcUncommonDelta { get; set; }
    //        public int wcRareDelta { get; set; }
    //        public int wcMythicDelta { get; set; }
    //        public List<ArtSkinsAdded> artSkinsAdded { get; set; }
    //        public List<object> artSkinsRemoved { get; set; }
    //        public List<object> voucherItemsDelta { get; set; }
    //    }

    public class AetherizedCard
    {
        public int grpId { get; set; }
        public bool addedToInventory { get; set; }
        public bool isGrantedFromDeck { get; set; }
        public float vaultProgress { get; set; }
        public int goldAwarded { get; set; }
        public int gemsAwarded { get; set; }
        public string set { get; set; }
    }

    public class Context
    {
        public string source { get; set; }
        public string sourceId { get; set; }
    }

    public class Update
    {
        public Delta delta { get; set; } = new Delta();
        public ICollection<AetherizedCard> aetherizedCards { get; set; } = Array.Empty<AetherizedCard>();
        public int xpGained { get; set; }
        public Context context { get; set; }
    }

    public class InventoryUpdatedRaw
    {
        public string context { get; set; }
        public ICollection<Update> updates { get; set; } = Array.Empty<Update>();

        // For MtgaProLogger
        public long timestamp { get; set; }
    }
}