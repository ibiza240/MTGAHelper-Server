using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTGAHelper.Entity;

namespace MTGAHelper.Web.UI
{
    public class FilesHashManager
    {
        public Dictionary<DataFileTypeEnum, uint> HashByType { get; private set; } = new Dictionary<DataFileTypeEnum, uint>();

        public void Init(string folderData)
        {
            var filesForTypes = Enum.GetValues(typeof(DataFileTypeEnum)).Cast<DataFileTypeEnum>()
                .Where(i => i != DataFileTypeEnum.Unknown)
                .Select(value => new { value, file = Path.Combine(folderData, $"{value}.json") });

            HashByType = filesForTypes.ToDictionary(i => i.value, i => Fnv1aHasher.To32BitFnv1aHash(File.ReadAllText(i.file)));
        }
    }
}
