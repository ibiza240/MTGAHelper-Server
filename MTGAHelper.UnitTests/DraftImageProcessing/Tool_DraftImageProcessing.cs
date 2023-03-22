//using System.Drawing;
//using System.IO;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using MTGAHelper.OCR.Tests.SetDraftingResources.IKO;

//namespace MTGAHelper.UnitTests.DraftImageProcessing
//{
//    [TestClass, Ignore("tool")]
//    public class Tool_DraftImageProcessing : TestsBase
//    {
//        [TestMethod]
//        public void DraftImageProcessing_GenerateTemplatesRaw()
//        {
//            var preparator = new ModelPreparor().Init(allCards.Values.ToArray());

//            //preparator.SaveCardNameImagesNormal();
//            preparator.SaveCardNameImagesCosmetics();

//            var folderTemplates = @"C:\Users\BL\source\repos\MTGAHelper\data\cardNameTemplates";
//            var filesNormal = Directory.GetFiles(folderTemplates).Select(i => Path.GetFileNameWithoutExtension(i))
//                .Where(i => i.Contains("_") == false)
//                .ToArray();

//            var missing = allCards
//                .Where(i => i.Value.set == "IKO" && i.Value.notInBooster == false)
//                .Where(i => filesNormal.Contains(i.Value.grpId.ToString()) == false)
//                .Select(i => i.Value.name)
//                .ToArray();

//            var m = string.Join("\r\n", missing);
//        }

//        [TestMethod]
//        public void DraftImageProcessing_Resize()
//        {
//            var paths = Directory.GetFiles(@"C:\Users\BL\source\repos\MTGAHelper\MTGAHelper.Tracker.DraftHelper\Resources");
//            foreach (var i in paths)
//            {
//                var newImage = new Bitmap(105, 116);
//                {
//                    var image = new Bitmap(i);
//                    using (var g = Graphics.FromImage(newImage))
//                    {
//                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
//                        g.DrawImage(image, 0, 0, newImage.Width, newImage.Height);
//                    }
//                    image.Dispose();
//                }
//                File.Delete(i);
//                newImage.Save(i);
//            }
//        }
//    }
//}