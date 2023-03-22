using CsvHelper;
using CsvHelper.Configuration;
using MTGAHelper.Entity;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MTGAHelper.Web.UI.Shared
{
    public class CollectionExporterPart
    {
        public string Code { get; private set; }
        public string Header { get; private set; }
        public Func<CardWithAmount, dynamic> GetValue { get; private set; }

        public CollectionExporterPart(string header, Func<CardWithAmount, dynamic> getValue)
        {
            Code = "$" + header.ToLower();
            Header = header;
            GetValue = getValue;
        }
    }

    public class CollectionExporter
    {
        static readonly Dictionary<string, CollectionExporterPart> validParts = new Dictionary<string, CollectionExporterPart> {
            { "$name", new CollectionExporterPart("Name", (CardWithAmount card) => card.Card.Name) },
            { "$amount", new CollectionExporterPart("Amount", (CardWithAmount card) => card.Amount) },
            { "$set", new CollectionExporterPart("Set", (CardWithAmount card) => card.Card.Set) },
            { "$number", new CollectionExporterPart("Number", (CardWithAmount card) => card.Card.Number) },
            { "$rarity", new CollectionExporterPart("Rarity", (CardWithAmount card) => card.Card.RarityStr) },
            { "$type", new CollectionExporterPart("Type", (CardWithAmount card) => card.Card.Type) },
            { "$cmc", new CollectionExporterPart("CMC", (CardWithAmount card) => card.Card.Cmc) },
            { "$color", new CollectionExporterPart("Color", (CardWithAmount card) => string.Join("", card.Card.Colors)) },
        };

        ICollection<string> validatedPartsToUse;

        public byte[] Export(ICollection<CardWithAmount> collection, bool header)
        {
            ICollection<dynamic> dynamicOutput = BuildDynamicResults(collection);

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new CsvWriter(new StreamWriter(memoryStream), new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = header,
                }))
                {
                    writer.WriteRecords(dynamicOutput);
                    writer.Flush();
                }
                return memoryStream.ToArray();
            }
        }

        ICollection<dynamic> BuildDynamicResults(ICollection<CardWithAmount> collection)
        {
            var results = new List<dynamic>();
            foreach (var c in collection)
            {
                var record = new ExpandoObject() as IDictionary<string, object>;

                foreach (var p in validatedPartsToUse)
                    record.Add(validParts[p].Header, validParts[p].GetValue(c));

                results.Add(record);
            }

            return results;
        }

        internal bool ValidateFormat(string format)
        {
            if (format == null)
                return false;

            var parts = format.Split(',').Select(i => i.Trim()).ToArray();
            var isValid = parts.Length > 0 && parts.All(i => validParts.Keys.Contains(i));

            if (isValid)
                validatedPartsToUse = parts;

            return isValid;
        }
    }
}
