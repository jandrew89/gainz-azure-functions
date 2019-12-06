using Newtonsoft.Json;
using System;

namespace Core.Services.Data
{
    public class Activity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Notes { get; set; }
        public int Order { get; set; }
        public Equipment Equipment { get; set; }
        public Set[] Sets { get; set; }
    }
}
