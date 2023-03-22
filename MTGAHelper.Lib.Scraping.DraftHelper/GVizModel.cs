using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Lib.Scraping.DraftHelper
{
    internal class GVizModel
    {
        public string version { get; set; }
        public string reqId { get; set; }
        public string status { get; set; }
        public string sig { get; set; }
        public Table table { get; set; }

        public class Table
        {
            public Col[] cols { get; set; }
            public Row[] rows { get; set; }
            public int parsedNumHeaders { get; set; }
        }

        public class Col
        {
            public string id { get; set; }
            public string label { get; set; }
            public string type { get; set; }
            public string pattern { get; set; }
        }

        public class Row
        {
            public C[] c { get; set; }
        }

        public class C
        {
            public float v { get; set; }
            public string f { get; set; }
        }
    }
}