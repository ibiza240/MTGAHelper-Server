﻿using System;

namespace MTGAHelper.Entity
{
    public class Set
    {
        public int Collation { get; set; }
        public string Scryfall { get; set; }
        public string Code { get; set; }
        public string Arenacode { get; set; }
        public int Tile { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
