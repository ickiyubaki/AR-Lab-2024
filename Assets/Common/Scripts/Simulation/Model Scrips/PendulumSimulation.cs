using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Scripts.Extensions;
using Common.Scripts.UI;
using Newtonsoft.Json;
using UnityEngine;
using DG.Tweening;
using Localization.Scripts;

namespace Common.Scripts.Simulation.Model_Scrips
{
    public class PendulumSimulationData : SimulationData
    { 
        public override string Time { get; set; }
        
        // angle between pendulum and vertical upright direction
        [JsonProperty(PropertyName = "theta") ] 
        public string Theta { get; set; }
        
        // rotation angle of the reaction wheel
        [JsonProperty(PropertyName = "phi")]
        public string Phi { get; set; }
    }

    public class PendulumSimulation : Simulation<PendulumSimulationData>
    {
        private const string PropellerTag = "rwpendulum_propeller";
        private const string ArmTag = "rwpendulum_arm";
        private const float StartStopSequenceTime = 3f;

        protected sealed override IEnumerable<string> GetComponentsTagNames()
        {
            return new[] { PropellerTag, ArmTag };
        }
        
        protected override string GetShareSubject() =>
            LocalizationManager.GetStringTableEntryOrDefault("PENDULUM_DATA", "Pendulum simulation data");
        
        protected sealed override IEnumerator Simulate(List<PendulumSimulationData> simulationData, 
            IReadOnlyDictionary<string, GameObject[]> components)
        {
            var propellers = components[PropellerTag];
            var arms = components[ArmTag];
            
            StartSequence(arms);
            yield return new WaitForSeconds(StartStopSequenceTime);

            var previousTime = 0m;
            var previousTheta = 0f;
            var previousPhi = 0f;

            foreach (var data in simulationData)
            {
                if (string.IsNullOrEmpty(data.Theta) || string.IsNullOrEmpty(data.Phi) || string.IsNullOrEmpty(data.Time))
                {
                    Debug.LogError("Simulation data contains null or empty values.");
                    continue;
                }
                var theta = float.Parse(data.Theta, CultureInfo.InvariantCulture.NumberFormat) * Mathf.Rad2Deg;
                var phi = float.Parse(data.Phi, CultureInfo.InvariantCulture.NumberFormat) * Mathf.Rad2Deg;
                var time = decimal.Parse(data.Time, CultureInfo.InvariantCulture);
                
                foreach (var arm in arms)
                {
                    arm.transform.Rotate(new Vector3(0, 0, theta - previousTheta));   
                }

                foreach (var propeller in propellers)
                {
                    propeller.transform.Rotate(new Vector3(0, 0,  phi - previousPhi));
                }
                
                yield return new WaitForSeconds((float)(time - previousTime));
                
                previousTheta = theta;
                previousPhi = phi;
                previousTime = time;
            }
            
            StopSequence(arms);
        }

        protected override IEnumerator DrawGraph(List<PendulumSimulationData> simulationData, decimal dataStep)
        {
            var graphInstance = Graph.Instance;
            var timeInterval = simulationData.LastOrDefault()?.Time;
            if (timeInterval == null) yield break;
            var numberOfSplits = Mathf.RoundToInt(float.Parse(timeInterval, CultureInfo.InvariantCulture.NumberFormat) /
                                                  DrawTimeStepInSeconds);
            var splitSimulationData = simulationData.Split(numberOfSplits);

            graphInstance.SetUpGraph(new LocalizationKeyValue("PENDULUM", "Pendulum"), "s", "");

            yield return new WaitForSeconds(StartStopSequenceTime);

            var dataArm = new GraphData(new LocalizationKeyValue("PENDULUM_ARM", "Arm"), Color.red,
                new List<float>());
            var dataPropeller = new GraphData(new LocalizationKeyValue("PENDULUM_PROPELLER", "Propeller"), Color.green,
                new List<float>());

            foreach (var batch in splitSimulationData)
            {
                foreach (var item in batch)
                {
                    dataArm.Points.Add(float.Parse(item.Theta, CultureInfo.InvariantCulture.NumberFormat));
                    dataPropeller.Points.Add(float.Parse(item.Phi, CultureInfo.InvariantCulture.NumberFormat));
                }

                graphInstance.DrawGraph(new List<GraphData> { dataArm, dataPropeller }, dataStep);

                yield return new WaitForSeconds(DrawTimeStepInSeconds);
            }
        }

        private void StartSequence(IEnumerable<GameObject> arms)
        {
            var sequence = DOTween.Sequence();
            
            foreach (var arm in arms)
            {
                sequence.Join(arm.transform.DOLocalRotate(new Vector3(0,0,360),StartStopSequenceTime));
            }
            
            sequence.Play();
        }

        private void StopSequence(IEnumerable<GameObject> arms)
        {
            var sequence = DOTween.Sequence();
            
            foreach (var arm in arms)
            {
                sequence.Join(arm.transform.DOLocalRotate(new Vector3(0,0,180), StartStopSequenceTime));
            }
            sequence.Play();
        }
    }
}