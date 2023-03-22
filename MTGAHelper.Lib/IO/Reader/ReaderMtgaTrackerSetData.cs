//using MTGAHelper.Entity;
//using Serilog;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text.RegularExpressions;

//namespace MTGAHelper.Lib.IO.Reader
//{
//    public class ReaderMtgaTrackerSetData
//    {
//        public ICollection<MtgaMappingItem> GetMappings(string folderSetData)
//        {
//            var files = Directory.GetFiles(folderSetData, "*.py", SearchOption.TopDirectoryOnly);
//            var res = new List<MtgaMappingItem>();
//            var regex = new Regex(@"pretty_name=""(.*?)"".*?set_id=""(.*?)"".*?rarity=""(.*?)"".*?set_number=(\d+).*?mtga_id=(\d+)", RegexOptions.Singleline | RegexOptions.Compiled);

//            foreach (var f in files)
//            {
//                var text = File.ReadAllText(f);
//                var matches = regex.Matches(text);
//                if (matches.Count > 0)
//                {
//                    foreach (Match m in matches)
//                        res.Add(new MtgaMappingItem
//                        {
//                            Name = m.Groups[1].Value,
//                            Set = m.Groups[2].Value == "DAR" ? "DOM" : m.Groups[2].Value,
//                            Rarity = m.Groups[3].Value,
//                            Number = m.Groups[4].Value,
//                            grpId = Convert.ToInt32(m.Groups[5].Value),
//                        });
//                }
//            }

//            return res;
//        }
//    }
//}