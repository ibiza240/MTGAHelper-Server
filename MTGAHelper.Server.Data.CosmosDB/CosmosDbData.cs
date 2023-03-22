using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTGAHelper.Server.Data.CosmosDB
{
    public class CosmosDbData<T>
    {
        // Partition Key
        public string UserId { get; set; }

        /// <summary>
        /// Unique Id for the data in the partition
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string DataKey { get; set; }

        public T Data { get; set; }
    }
}
