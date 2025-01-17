using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Scripts.Extensions;
using Common.Scripts.UI;
using Localization.Scripts;
using Newtonsoft.Json;
using UnityEngine;

namespace Common.Scripts.Simulation.Model_Scrips
{
    public class HydraulicSimulationData : SimulationData
    {
        public override string Time { get; set; }
        
        [JsonProperty(PropertyName = "h1")] 
        public string H1 { get; set; }

        [JsonProperty(PropertyName = "h2")] 
        public string H2 { get; set; }

        [JsonProperty(PropertyName = "h3")] 
        public string H3 { get; set; }
    }

    public class HydraulicSimulation : Simulation<HydraulicSimulationData>
    {
        private const string Tank1Tag = "hydraulic_tank1";
        private const string Tank2Tag = "hydraulic_tank2";
        private const string Tank3Tag = "hydraulic_tank3";
        private const string Valve1Tag = "hydraulic_valve1";
        private const string Valve2Tag = "hydraulic_valve2";
        private const string Valve3Tag = "hydraulic_valve3";
        private const string Valve4Tag = "hydraulic_valve4";
        private const string Valve5Tag = "hydraulic_valve5";
        private const string Tube1Tag = "hydraulic_tube1";
        private const string Tube2Tag = "hydraulic_tube2";
        private const string Tube3Tag = "hydraulic_tube3";
        private const string Tube4Tag = "hydraulic_tube4";
        private const string Tube5Tag = "hydraulic_tube5";
        private const string Tube6Tag = "hydraulic_tube6";
        private const string Tube7Tag = "hydraulic_tube7";
        private const string Tube8Tag = "hydraulic_tube8";
        
        private const float StartStopSequenceTime = 1.5f;
        private const float MaxWaterLevel = 50f;

        protected override IEnumerable<string> GetComponentsTagNames()
        {
            return new[]
            {
                Tank1Tag, Tank2Tag, Tank3Tag, Valve1Tag, Valve2Tag, Valve3Tag, Valve4Tag, Valve5Tag,
                Tube1Tag, Tube2Tag, Tube3Tag, Tube4Tag, Tube5Tag, Tube6Tag, Tube7Tag, Tube8Tag
            };
        }
        
        protected override string GetShareSubject() =>
            LocalizationManager.GetStringTableEntryOrDefault("HYDRAULIC_DATA", "Hydraulic simulation data");

        protected sealed override IEnumerator Simulate(List<HydraulicSimulationData> simulationData,
            IReadOnlyDictionary<string, GameObject[]> components)
        { 
            var (outputValveGroups, betweenValveGroups, tankGroups, shorterTubeGroups, longerTubeGroups) =
                GetComponentGroups(components);

            var startSequence = new HydraulicStartSequence(outputValveGroups.Concat(betweenValveGroups).ToArray(),
                shorterTubeGroups, longerTubeGroups, StartStopSequenceTime);
            var stopSequence = new HydraulicStopSequence(
                outputValveGroups.Select(vg => vg.valves).ToArray(),
                betweenValveGroups.Select(vg => vg.valves).ToArray(),
                tankGroups.Select(tg => tg.tanks).ToArray(),
                shorterTubeGroups, longerTubeGroups, StartStopSequenceTime);
            
            startSequence.PlayStartSequence();
            yield return new WaitForSeconds(StartStopSequenceTime);

            var maxDataWaterLevel = FindMaxWaterLevel(simulationData);
            var waterLevelBias = maxDataWaterLevel > MaxWaterLevel ? MaxWaterLevel / maxDataWaterLevel : 1f;
            var previousTime = 0m;

            foreach (var data in simulationData)
            {
                var time = decimal.Parse(data.Time, CultureInfo.InvariantCulture.NumberFormat);

                tankGroups[0].height = float.Parse(data.H1, CultureInfo.InvariantCulture.NumberFormat) * waterLevelBias;
                tankGroups[1].height = float.Parse(data.H2, CultureInfo.InvariantCulture.NumberFormat) * waterLevelBias;
                tankGroups[2].height = float.Parse(data.H3, CultureInfo.InvariantCulture.NumberFormat) * waterLevelBias;
                
                foreach (var (tanks, height) in tankGroups)
                {
                    FillTanksWithWater(tanks, height);
                }

                yield return new WaitForSeconds((float) (time - previousTime));

                previousTime = time;
            }

            stopSequence.PlayStopSequence();
        }

        protected override IEnumerator DrawGraph(List<HydraulicSimulationData> simulationData, decimal dataStep)
        {
            var graphInstance = Graph.Instance;
            var timeInterval = simulationData.LastOrDefault()?.Time;
            if (timeInterval == null) yield break;
            var numberOfSplits = Mathf.RoundToInt(float.Parse(timeInterval, CultureInfo.InvariantCulture.NumberFormat) /
                                                  DrawTimeStepInSeconds);
            var splitSimulationData = simulationData.Split(numberOfSplits);

            graphInstance.SetUpGraph(new LocalizationKeyValue("HYDRAULIC", "Hydraulic system"), "s", "cm");

            yield return new WaitForSeconds(StartStopSequenceTime);

            var dataH1 = new GraphData(new LocalizationKeyValue("H1", "H1"), Color.red,
                new List<float>());
            var dataH2 = new GraphData(new LocalizationKeyValue("H2", "H2"), Color.green,
                new List<float>());
            var dataH3 = new GraphData(new LocalizationKeyValue("H3", "H3"), Color.yellow,
                new List<float>());

            foreach (var batch in splitSimulationData)
            {
                foreach (var item in batch)
                {
                    dataH1.Points.Add(float.Parse(item.H1, CultureInfo.InvariantCulture.NumberFormat));
                    dataH2.Points.Add(float.Parse(item.H2, CultureInfo.InvariantCulture.NumberFormat));
                    dataH3.Points.Add(float.Parse(item.H3, CultureInfo.InvariantCulture.NumberFormat));
                }

                graphInstance.DrawGraph(new List<GraphData> { dataH1, dataH2, dataH3 }, dataStep);

                yield return new WaitForSeconds(DrawTimeStepInSeconds);
            }
        }

        // Helper methods

        private float CalculateValveRotation(string schemeVar, bool reverse = false)
        {
            var openingPercentage = ExperimentData.InputParameters.Single(ip => ip.SchemaVar == schemeVar).DefaultValue
                .Single().Name;
            var parsedPercentage = float.Parse(openingPercentage, CultureInfo.InvariantCulture.NumberFormat);
            return ((reverse ? 100 - parsedPercentage : parsedPercentage) / 100) * 90;
        }
        
        private void FillTanksWithWater(IEnumerable<GameObject> tanks, float height)
        {
            foreach (var tank in tanks)
            {
                var localScale = tank.transform.localScale;
                tank.transform.localScale = new Vector3(localScale.x, height, localScale.z);
            }
        }
        
        private float FindMaxWaterLevel(List<HydraulicSimulationData> data)
        {
            var maxWaterLevel = 0f;
            foreach (var item in data)
            {
                var h1 = float.Parse(item.H1, CultureInfo.InvariantCulture.NumberFormat);
                var h2 = float.Parse(item.H2, CultureInfo.InvariantCulture.NumberFormat);
                var h3 = float.Parse(item.H2, CultureInfo.InvariantCulture.NumberFormat);

                if (h1 > maxWaterLevel || h2 > maxWaterLevel || h3 > maxWaterLevel)
                {
                    maxWaterLevel = Math.Max(h1, Math.Max(h2, h3));
                }
            }

            return maxWaterLevel;
        }

        private ((GameObject[] valves, float rotation)[] outputValveGroups, (GameObject[] valves, float rotation)[]
            betweenValveGroups, (GameObject[] tanks, float height)[] tankGroups, (GameObject[] tubes, float duration)[]
            tubeGroups, (GameObject[] tubes, float duration)[])
            GetComponentGroups(IReadOnlyDictionary<string, GameObject[]> components)
        {
            var outputValveGroups = new (GameObject[] valves, float rotation)[]
            {
                (components[Valve1Tag], CalculateValveRotation("V1")),
                (components[Valve2Tag], CalculateValveRotation("V2")),
                (components[Valve3Tag], CalculateValveRotation("V3")),
            };

            var betweenValveGroups = new (GameObject[] valves, float rotation)[]
            {
                (components[Valve4Tag], CalculateValveRotation("V13", true)),
                (components[Valve5Tag], CalculateValveRotation("V23", true))
            };

            var tankGroups = new (GameObject[] tanks, float height)[]
            {
                (components[Tank1Tag], 0),
                (components[Tank2Tag], 0),
                (components[Tank3Tag], 0)
            };

            var longerTubeGroups = new (GameObject[] tubes, float duration)[]
            {
                (components[Tube2Tag], 0.185f),
                (components[Tube4Tag], 0.185f),
                (components[Tube6Tag], 0.3f),
                (components[Tube8Tag], 0.405f)
            };

            var shorterTubeGroups = new (GameObject[] tubes, float duration)[]
            {
                (components[Tube1Tag], 0.25f),
                (components[Tube3Tag], 0.25f),
                (components[Tube5Tag], 0.3f),
                (components[Tube7Tag], 0.1f)
            };

            return (outputValveGroups, betweenValveGroups, tankGroups, shorterTubeGroups, longerTubeGroups);
        }
    }
}