using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTGAHelper.UnitTests.DraftImageProcessing.SetDraftingResources.IKO
{
    public class ScreenOfCards
    {
        public string Filename { get; set; }
        public Bitmap CardsShown { get; }
        public Point FirstCardArtLocation { get; } = new Point(60, 443);
        public Point LastCardArtLocation { get; } = new Point(3461, 1668);
        public Size CardSize { get; } = new Size(209, 232);
        public int NbCols { get; } = 9;
        public int NbRows { get; } = 3;

        public float StepX => (LastCardArtLocation.X - FirstCardArtLocation.X) / (float)(NbCols - 1);
        public float StepY => (LastCardArtLocation.Y - FirstCardArtLocation.Y) / (float)(NbRows - 1);

        public ScreenOfCards(string filepath)
        {
            Filename = Path.GetFileNameWithoutExtension(filepath);
            CardsShown = new Bitmap(filepath);
        }
    }
}