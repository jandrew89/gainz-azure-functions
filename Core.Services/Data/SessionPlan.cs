using Core.Services.Data.Dto;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Core.Services.Data
{
    public class SessionPlan
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string SessionType { get; set; }
        public string SessionPlanName { get; set; }
        public List<EquipmentDto> Equipment { get; set; }
    }
}
