using Newtonsoft.Json;

namespace Core.Services.Data
{
    public class SessionType
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
