using Newtonsoft.Json;

namespace Core.Services.Data
{
    public class SessionType
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
