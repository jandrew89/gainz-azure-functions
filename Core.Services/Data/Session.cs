using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Core.Services.Data
{
    public class Session
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public int Weight { get; set; }
        public DateTime SessionDate { get; set; }
        public string SessionType { get; set; }
        public List<Activity> Activities { get; set; }
    }
}
