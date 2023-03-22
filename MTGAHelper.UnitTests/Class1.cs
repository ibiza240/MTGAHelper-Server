using Microsoft.VisualStudio.TestTools.UnitTesting;
using MTGAHelper.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MTGAHelper.UnitTests
{
    public class Asset
    {
        public string Name { get; set; }
        public string AssetType { get; set; }
        public int Length { get; set; }
        public int CompressedLength { get; set; }
        public string Hash { get; set; }
        public string MD5 { get; set; }
        //public List<object> Dependencies { get; set; }
    }

    public class RootObject
    {
        public int FormatVersion { get; set; }
        public List<Asset> Assets { get; set; }
    }

    public partial class Tools
    {
        //[TestMethod]
        //public void Test_Protobuf()
        //{
        //    var payload = "CDUQAsICCAoBARoDCNAC";
        //    var bytes = Convert.FromBase64String(payload);
        //    var test = ClientToGREMessage.Parser.ParseFrom(bytes);
        //}

        [TestMethod]
        public void Test_EmailToHash()
        {
            string EmailToHash(string email)
            {
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = Encoding.ASCII.GetBytes(email.ToLower());
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    var emailHash = hashBytes.ToBase32String(false);
                    return emailHash;
                }
            };

            var test = EmailToHash("");
        }

        [TestMethod]
        public void Get_MTGA_Assets_ALL()
        {
            // From the log file: EndpointHashPath
            var endpointMtga = @"https://assets.mtgarena.wizards.com/External_2119_767741.mtga";
            DownloadAssets(false, @"D:\repos\MTGAHelper\data\all", endpointMtga);
        }

        [TestMethod]
        public void Get_MTGA_Assets()
        {
            // From the log file: EndpointHashPath
            var url = @"https://assets.mtgarena.wizards.com/External_2213_786053.mtga";
            string id = null;
            using (var w = new WebClient())
            {
                // Get the version id
                id = w.DownloadString(url);
                if (id == null) Assert.Fail();

                // Get the manifest
                url = $@"https://assets.mtgarena.wizards.com/Manifest_{id}.mtga";
                var gzipdata = w.DownloadData(url);

                // Inside this gzip will be a JSON file
                var json = System.Text.Encoding.Default.GetString(decompress(gzipdata));

                // Look up for data_cards inside the text
                // to determine the url of cards data
                var match = Regex.Match(json, @"""Name"": ""(data_cards_.*?.mtga)""");
                if (match.Success == false || match.Groups.Count < 2) Assert.Fail();
                var datacards = match.Groups[1].Value;

                // Again, gzipped
                url = $@"https://assets.mtgarena.wizards.com/{datacards}";
                gzipdata = w.DownloadData(url);

                // Inside this gzip you have the cards data.
                var jsonCards = System.Text.Encoding.Default.GetString(decompress(gzipdata));

                var path = Path.Combine(@"C:\Users\BL\source\repos\MTGAHelper\data", "data_cards.mtga");
                File.WriteAllText(path, jsonCards);
            }
        }
    }
}