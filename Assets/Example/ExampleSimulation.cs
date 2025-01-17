using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Scripts.Extensions;
using Common.Scripts.Simulation;
using Common.Scripts.UI;
using Localization.Scripts;
using Newtonsoft.Json;
using UnityEngine;

namespace Example
{
    public class ExampleSimulationData : SimulationData
    {
        public override string Time { get; set; }

        // príklad premennej, ktorá príde z API
        [JsonProperty(PropertyName = "parameter")]
        public string Parameter { get; set; }
    }

    public class ExampleSimulation : Simulation<ExampleSimulationData>
    {
        private const string ComponentTag = "component";

        /// <summary>
        /// Každý komponent, ktorý chceme nejakým sposobom simulavať musí mať pridelený TAG.
        /// Podľa príslušného tagu budú následne jednotlivé komponenty vyhladané.
        /// </summary>
        /// <returns> Kolekciu všetkých tagov, ktoré majú byť použité na vyhľadanie komponentov na scéne </returns>
        protected override IEnumerable<string> GetComponentsTagNames()
        {
            return new[] { ComponentTag };
        }

        /// <inheritdoc />
        protected override IEnumerator Simulate(List<ExampleSimulationData> simulationData,
            IReadOnlyDictionary<string, GameObject[]> components)
        {
            var componentObjects = components[ComponentTag];
            var previousTime = 0m;

            foreach (var data in simulationData)
            {
                var time = decimal.Parse(data.Time, CultureInfo.InvariantCulture);

                // TU simulačná logika

                yield return new WaitForSeconds((float)(time - previousTime));
                previousTime = time;
            }
        }

        /// <inheritdoc />
        protected override IEnumerator DrawGraph(List<ExampleSimulationData> simulationData, decimal dataStep)
        {
            var graphInstance = Graph.Instance;
            var timeInterval = simulationData.LastOrDefault()?.Time;

            if (timeInterval == null) yield break;
            // Pre zníženie zaťaženia apliakcie, odporúčam graf vykreslovať každú sekundu.
            // Dáta sú preto rozdelené na batche, ktoré sú postupne vykreslované
            var numberOfSplits = Mathf.RoundToInt(float.Parse(timeInterval, CultureInfo.InvariantCulture.NumberFormat) /
                                                  DrawTimeStepInSeconds);
            var splitSimulationData = simulationData.Split(numberOfSplits);

            // Inicializácia grafu a natavenie názvov
            graphInstance.SetUpGraph(new LocalizationKeyValue("GRAPH", "Graph"), "x", "y");

            // Vytvorenie premennej, ktorá reprezentuje jedno dátove pole
            var dataParameter = new GraphData(new LocalizationKeyValue("PARAMETER", "Parameter"), Color.red,
                new List<float>());

            foreach (var batch in splitSimulationData)
            {
                // Postupné pridávanie batchov
                dataParameter.Points.AddRange(batch.Select(d =>
                    float.Parse(d.Parameter, CultureInfo.InvariantCulture.NumberFormat)));

                // Vykreslenie grafu
                graphInstance.DrawGraph(new List<GraphData> { dataParameter }, dataStep);

                // Defaultné vykreslovanie každú sekundu
                yield return new WaitForSeconds(DrawTimeStepInSeconds);
            }
        }
    }
}