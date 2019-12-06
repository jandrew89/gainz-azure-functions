using Newtonsoft.Json;
using System.Collections.Generic;

namespace Core.Services.Data
{
    public class Employee
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string Company { get; set; }
        public string Designation { get; set; }
        public string Cityname { get; set; }
        public List<UserSession> Sessions { get; set; }
    }
}