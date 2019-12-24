using Newtonsoft.Json;

namespace Core.Services.Data.Dto
{
    public class EquipmentDto
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string name { get; set; }
    }
}
