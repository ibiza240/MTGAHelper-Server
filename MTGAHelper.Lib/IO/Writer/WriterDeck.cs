using MTGAHelper.Entity;
using MTGAHelper.Lib.IO.Writer.WriterDeckTypes;
using System;
using System.IO;

namespace MTGAHelper.Lib.IO.Writer
{
    public interface IWriterDeck
    {
        void Save(IDeck deck, string directory, bool alertIfExists = false);
        string ToText(IDeck deck);
    }

    public class WriterDeck : IWriterDeck
    {
        IWriterDeckMtga writerDeckMtga;
        //IWriterDeckAverageArchetype writerDeckAverageArchetype;

        public WriterDeck(IWriterDeckMtga writerDeckMtga/*, IWriterDeckAverageArchetype writerDeckAverageArchetype*/)
        {
            this.writerDeckMtga = writerDeckMtga;
            //this.writerDeckAverageArchetype = writerDeckAverageArchetype;
        }

        public void Save(IDeck deck, string directory, bool alertIfExists = false)
        {
            //if (deck is DeckAverageArchetype)
            //    writerDeckAverageArchetype.Write((DeckAverageArchetype)deck, directory);
            //else
                writerDeckMtga.Write(deck, directory, alertIfExists);
        }

        public string ToText(IDeck deck)
        {
            //if (deck is DeckAverageArchetype)
            //    throw new InvalidOperationException("This Deck type (DeckAverageArchetype is deprecated)");
            //else
                return writerDeckMtga.ToText(deck);
        }
    }
}
