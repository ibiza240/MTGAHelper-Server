//using MTGAHelper.Entity;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.IO;
//using System.Linq;

//namespace MTGAHelper.OCR.Tests.SetDraftingResources.IKO
//{
//    public class ModelPreparor
//    {
//        private readonly string folderInput = @"C:\Users\BL\source\repos\MTGAHelper\MTGAHelper.UnitTests\DraftImageProcessing\SetDraftingResources\IKO";
//        private readonly string folderTemplates = @"C:\Users\BL\source\repos\MTGAHelper\data\cardNameTemplates";

//        ICollection<Card> cardsIkoNormal;

//        public ModelPreparor Init(ICollection<Card> allCards)
//        {
//            cardsIkoNormal = allCards
//                .Where(i => i.set == "IKO")
//                .Where(i => int.TryParse(i.number, out int n) && n <= 275)
//                .OrderBy(i => Order(i))
//                .ToArray();

//            return this;
//        }

//        public void SaveCardNameImagesCosmetics()
//        {
//            var screenshots = Directory.GetFiles(folderInput)
//                .Where(i => Path.GetFileName(i).StartsWith("page") && i.Contains("cosmetics"))
//                .OrderBy(i => i)
//                .Select(i => new ScreenOfCards(i))
//                .ToArray();

//            var cardsOrdered = File.ReadAllText(Path.Combine(folderInput, "cardlist.txt")).Split(Environment.NewLine)
//                .Where(i => i != "Zilortha, Strength Incarnate")        // No Godzilla
//                .Where(i => int.TryParse(i, out int test) == false)     // No lands
//                .ToArray();

//            int iScreen = 0;
//            int nbCardsProcessed = 0;
//            foreach (var screen in screenshots)
//            {
//                var nbCardsOnScreen = 27;
//                var cardsOnScreen = cardsOrdered.Skip(nbCardsProcessed).Take(nbCardsOnScreen).ToList();
//                nbCardsProcessed += nbCardsOnScreen;

//                var partialResult = ParseCardsScreen(screen, cardsOnScreen);

//                foreach (var r in partialResult)
//                {
//                    if (int.TryParse(r.Key, out int cardId) == false)
//                    {
//                        cardId = cardsIkoNormal.Single(i => i.name == r.Key).grpId;
//                    }

//                    r.Value.Save(Path.Combine(folderTemplates, $"{cardId}_1.bmp"));
//                }
//                iScreen++;
//            }
//        }

//        public void SaveCardNameImagesNormal()
//        {
//            var screenshots = Directory.GetFiles(folderInput)
//                .Where(i => Path.GetFileName(i).StartsWith("page") && i.Contains("cosmetics") == false)
//                .OrderBy(i => i)
//                .Select(i => new ScreenOfCards(i))
//                .ToArray();

//            var cardsOrdered = File.ReadAllText(Path.Combine(folderInput, "cardlist.txt")).Split(Environment.NewLine);

//            var result = new Dictionary<int, Bitmap>();
//            int iScreen = 0;
//            int nbCardsProcessed = 0;
//            foreach (var screen in screenshots)
//            {
//                var nbCardsOnScreen = screen.Filename == "page10" ? 17 : 27;
//                var cardsOnScreen = cardsOrdered.Skip(nbCardsProcessed).Take(nbCardsOnScreen).ToList();
//                nbCardsProcessed += nbCardsOnScreen;

//                var partialResult = ParseCardsScreen(screen, cardsOnScreen);

//                foreach (var r in partialResult)
//                {
//                    if (int.TryParse(r.Key, out int cardId) == false)
//                    {
//                        cardId = cardsIkoNormal.Single(i => i.name == r.Key).grpId;
//                    }

//                    if (cardId != 71341)    // Don't save Godzilla
//                    {
//                        r.Value.Save(Path.Combine(folderTemplates, $"{cardId}.bmp"));
//                    }
//                }
//                iScreen++;
//            }
//        }

//        private int Order(Card i)
//        {
//            // Used to approximately build cardlist.txt
//            // (Still validated manually)

//            if (i.colors.Count == 1)
//            {
//                return i.colors.Single() switch
//                {
//                    "W" => 1,
//                    "U" => 2,
//                    "B" => 3,
//                    "R" => 4,
//                    "G" => 5,
//                    _ => 0,
//                };
//            }
//            else if (i.colors.Count == 0)
//                return 99;
//            else
//            {
//                return string.Join("", i.colors) switch
//                {
//                    "WU" => 10,
//                    "WB" => 11,
//                    "UB" => 12,
//                    "UR" => 13,
//                    "BR" => 14,
//                    "BG" => 15,
//                    "RG" => 16,
//                    "WR" => 17,
//                    "WG" => 18,
//                    "UG" => 19,
//                    "WBR" => 20,
//                    "URG" => 21,
//                    "WBG" => 22,
//                    "WUR" => 23,
//                    "UBG" => 24,
//                    _ => 0
//                };
//            }
//        }

//        Dictionary<string, Bitmap> ParseCardsScreen(ScreenOfCards screen, IList<string> cardsOnScreen)
//        {
//            var cardImages = new Dictionary<string, Bitmap>();

//            int iCard = 0;
//            for (int iRow = 0; iRow < 3; iRow++)
//            {
//                for (int iCol = 0; iCol < 9; iCol++)
//                {
//                    var location = new Point(screen.FirstCardArtLocation.X + (int)(iCol * screen.StepX), screen.FirstCardArtLocation.Y + (int)(iRow * screen.StepY));
//                    var cardImage = GetCardImage(screen.CardsShown, location, screen.CardSize);
//                    cardImages.Add(cardsOnScreen[iCard], cardImage);

//                    iCard++;
//                    if (iCard >= cardsOnScreen.Count)
//                        // For screens with less than 27 cards
//                        return cardImages;
//                }
//            }

//            return cardImages;
//        }

//        Bitmap GetCardImage(Bitmap cardsShown, Point location, Size size)
//        {
//            var cardImage = new Bitmap(size.Width, size.Height);
//            using (Graphics gfx = Graphics.FromImage(cardImage))
//            {
//                gfx.DrawImage(cardsShown, -location.X - Tracker.DraftHelper.Shared.Constants.CARD_ART_OFFSET_X, -location.Y);
//            }

//            return cardImage;
//        }
//    }
//}