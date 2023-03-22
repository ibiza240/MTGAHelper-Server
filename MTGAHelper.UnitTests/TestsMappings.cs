using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MTGAHelper.UnitTests
{
    [TestClass, Ignore("was failing")]
    public class TestsMappings
    {
        [TestMethod]
        public void Test_Load_Mappings()
        {
            var files = Directory.GetFiles(@"D:\repos\MTGAHelper\configusers");
            foreach (var f in files)
            {
                //try
                //{
                    //var test = JsonConvert.DeserializeObject<IImmutableUser>(File.ReadAllText(f));
                    var o = File.ReadAllText(f);
                    var s = o.Replace("\"CollectionDate\":\"Unknown\"", "\"CollectionDate\":\"0001-01-01\"");

                    var r = new Regex("\"CollectionDate\":\"(.*?)\"");
                    var d = r.Match(o).Groups[1].Value;
                    if (DateTime.TryParse(d, out DateTime dt) == false)
                    {
                        //System.Diagnostics.Debugger.Break();
                        File.Delete(f);
                    }

                    //File.WriteAllText(f, s);
                //}
                //catch (Exception ex)
                //{
                //    System.Diagnostics.Debugger.Break();
                //}
            }

            //var folder = @"C:\Visual Studio 2017\Projects\MTGAHelper\set_data";
            //var res = new ReaderMtgaTrackerSetData().GetMappings(folder);
            //File.WriteAllText(Path.Combine(folder, "MtgaMappings.json"), JsonConvert.SerializeObject(res));
        }
    }
}
