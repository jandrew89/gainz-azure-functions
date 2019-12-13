using Newtonsoft.Json;
using System.Collections.Generic;

namespace Core.Services.Data
{
    public class Equipment
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string name { get; set; }
        public List<SessionType> SessionTypes { get; set; }
    }
}
