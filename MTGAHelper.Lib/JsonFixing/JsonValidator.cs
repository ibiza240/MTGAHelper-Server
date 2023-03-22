using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib.JsonFixing
{
    public class JsonValidator
    {
        public bool IsValid(string json)
        {
            try
            {
                JsonConvert.DeserializeObject(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public (bool success, string result) TryToFix(string json)
        {
            // Apply manual fix in debug
            json = System.IO.File.ReadAllText(@"D:\repos\MTGAHelper\fix.txt");

            try
            {
                JsonConvert.DeserializeObject(json);
                return (true, json);
            }
            catch
            {
                return (false, json);
            }
        }
    }
}
