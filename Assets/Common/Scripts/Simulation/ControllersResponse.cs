using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Scripts.Simulation
{
    public class ControllersResponse
    {
        [JsonProperty("data")]
        public List<Controller> Data { get; set; }
    }
}