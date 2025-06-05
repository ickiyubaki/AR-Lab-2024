using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Scripts.Simulation
{
    public class SimulationResponse<T>
    {
        [JsonProperty("simulation")]
        public List<T> Simulation { get; set; }
    }
}