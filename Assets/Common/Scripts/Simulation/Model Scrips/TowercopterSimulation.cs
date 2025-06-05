using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.Scripts.Extensions;
using Common.Scripts.UI;
using Common.Scripts.Utils;
using DG.Tweening;
using Localization.Scripts;
using Newtonsoft.Json;
using UnityEngine;

namespace Common.Scripts.Simulation.Model_Scrips
{
    public class TowercopterSimulationData : SimulationData
    {
        public override string Time { get; set; }
        
        // [JsonProperty(PropertyName = "reg")] 
        // public string Reg { get; set; }

        [JsonProperty(PropertyName = "height")]
        public string Height { get; set; }
    }

    public class TowercopterSimulation : Simulation<TowercopterSimulationData>
    {
        private const string EngineTag = "towercopter_engine";
        private const string PlatformTag = "towercopter_platform";
        private const string PropellerTag = "towercopter_propeller";
        private const string CableTag = "towercopter_cable";
        private const float RotationSpeedDecrease = 540f;
        private const float MaxRotationSpeed = 1080f;
        private const float MaxHeight = 80f;
        
        private bool _slowDown;
        private bool _enablePropellerRotation;
        private float _rotationSpeed = MaxRotationSpeed;

        private CableBezierCurveRenderer _cableBezierCurveRenderer;
        private List<GameObject> _propellers = new List<GameObject>();

        private Dictionary<GameObject, (Vector3[] localPositions, float sideMovementRation)>
            _defaultCableParameters;

        private void Start()
        {
            _cableBezierCurveRenderer = new CableBezierCurveRenderer();
        }

        protected override IEnumerable<string> GetComponentsTagNames()
        {
            return new[] { EngineTag, PlatformTag, PropellerTag, CableTag };
        }
        
        protected override string GetShareSubject() =>
            LocalizationManager.GetStringTableEntryOrDefault("TOWERCOPTER_DATA", "Towercopter simulation data");

        protected override IEnumerator Simulate(List<TowercopterSimulationData> simulationData,
            IReadOnlyDictionary<string, GameObject[]> components)
        {
            var previousTime = 0m;
            var previousHeight = 0f;
            var cables = components[CableTag];
            var enginesAndPlatforms = components[EngineTag].Concat(components[PlatformTag]).ToArray();
            var highestHeight =
                simulationData.Max(d => float.Parse(d.Height, CultureInfo.InvariantCulture.NumberFormat));
            var heightBias = highestHeight > MaxHeight ? MaxHeight / highestHeight : 1f;
            
            _propellers = components[PropellerTag].ToList();
            _defaultCableParameters = GetDefaultCableParameters(cables);
            
            ResetPropellerRotation();

            foreach (var data in simulationData)
            {
                if (string.IsNullOrEmpty(data.Time) || string.IsNullOrEmpty(data.Height))
                {
                    Debug.LogError("TowercopterSimulation: Simulation data contains null or empty values (Time, Height). Skipping this entry.");
                    continue;
                }
                var time = decimal.Parse(data.Time, CultureInfo.InvariantCulture.NumberFormat);
                var height = (float.Parse(data.Height, CultureInfo.InvariantCulture.NumberFormat) / 100) * heightBias;
                
                foreach (var component in enginesAndPlatforms)
                {
                    var localPosition = component.transform.localPosition;
                    component.transform.localPosition = new Vector3(localPosition.x, height, localPosition.z);
                }

                foreach (var cable in cables)
                {
                    MoveCablesUp(cable.GetComponent<TubeRenderer>(), height,
                        (height - previousHeight) * _defaultCableParameters[cable].sideMovementRation);
                }

                yield return new WaitForSeconds((float) (time - previousTime));
                previousTime = time;
                previousHeight = height;
            }

            StopSequence(enginesAndPlatforms, cables);
        }

        protected override IEnumerator DrawGraph(List<TowercopterSimulationData> simulationData, decimal dataStep)
        {
            var graphInstance = Graph.Instance;
            var timeInterval = simulationData.LastOrDefault()?.Time;
            if (timeInterval == null) yield break;
            var numberOfSplits = Mathf.RoundToInt(float.Parse(timeInterval, CultureInfo.InvariantCulture.NumberFormat) /
                                                  DrawTimeStepInSeconds);
            var splitSimulationData = simulationData.Split(numberOfSplits);

            graphInstance.SetUpGraph(new LocalizationKeyValue("TOWERCOPTER", "Towercopter"), "s", "cm");

            yield return new WaitForSeconds(1f);

            var dataHeight = new GraphData(new LocalizationKeyValue("TOWERCOPTER_HEIGHT", "Flight height"), Color.red,
                new List<float>());

            foreach (var batch in splitSimulationData)
            {
                dataHeight.Points.AddRange(batch
                    .Where(d => !string.IsNullOrEmpty(d.Height))
                    .Select(d => float.Parse(d.Height, CultureInfo.InvariantCulture.NumberFormat)));

                graphInstance.DrawGraph(new List<GraphData> { dataHeight }, dataStep);

                yield return new WaitForSeconds(DrawTimeStepInSeconds);
            }
        }

        private void StopSequence(IEnumerable<GameObject> movingComponents, IEnumerable<GameObject> cables)
        {
            var sequence = DOTween.Sequence();

            foreach (var component in movingComponents)
            {
                sequence.Join(component.transform.DOLocalMove(new Vector3(0, 0, 0), 2f));
            }

            foreach (var cable in cables)
            {
                sequence.Join(MoveCablesDown(cable.GetComponent<TubeRenderer>()));
            }

            _slowDown = true;
            sequence.Play().OnComplete(() => _enablePropellerRotation = false);
        }

        private void Update()
        {
            if (_enablePropellerRotation)
            {
                if (!_slowDown)
                {
                    foreach (var propeller in _propellers)
                    {
                        propeller.transform.Rotate(0, 0, _rotationSpeed * Time.deltaTime);
                    }
                }
                else if (_slowDown)
                {
                    if (_rotationSpeed >= 0)
                    {
                        _rotationSpeed -= RotationSpeedDecrease * Time.deltaTime;

                        foreach (var propeller in _propellers)
                        {
                            propeller.transform.Rotate(0, 0, _rotationSpeed * Time.deltaTime);
                        }
                    }
                }
            }
        }

        // Helper methods

        private void MoveCablesUp(TubeRenderer tubeRenderer, float height, float moveStep)
        {
            var tubeGameObject = tubeRenderer.gameObject;
            var points = tubeGameObject.transform.GetComponentsInChildren<Transform>()
                .Where(go => go.gameObject != tubeGameObject).ToArray();
            var defaultCablePositions = _defaultCableParameters[tubeGameObject].localPositions;

            // pohybu kablov (ich napinanie) je simulovany tak, ze bod 2 na kabli
            // postupne presuvame co najblizsie k stredovemu bodu medzi bodmi 1 a 3 
            points[1].localPosition = Vector3.MoveTowards(points[1].localPosition,
                Vector3.Lerp(points[0].localPosition, points[2].localPosition, 0.5f), moveStep);
            points[2].localPosition = new Vector3(defaultCablePositions[2].x + height, points[2].localPosition.y,
                points[2].localPosition.z);
            points[3].localPosition = new Vector3(defaultCablePositions[3].x + height, points[3].localPosition.y,
                points[3].localPosition.z);
            points[4].localPosition = new Vector3(defaultCablePositions[4].x + height, points[4].localPosition.y,
                points[4].localPosition.z);

            _cableBezierCurveRenderer.DrawCurvedTubeLine(tubeRenderer,
                points[0].localPosition, points[1].localPosition,
                points[2].localPosition, points[3].localPosition,
                points[4].localPosition);
        }

        private Sequence MoveCablesDown(TubeRenderer tubeRenderer)
        {
            var sequence = DOTween.Sequence();
            var tubeGameObject = tubeRenderer.gameObject;
            var points = tubeGameObject.transform.GetComponentsInChildren<Transform>()
                .Where(t => t.gameObject != tubeGameObject).ToArray();
            var defaultCablePositions = _defaultCableParameters[tubeGameObject].localPositions;

            sequence.Join(points[1].transform.DOLocalMove(defaultCablePositions[1], 2f))
                .Join(points[2].transform.DOLocalMove(defaultCablePositions[2], 2f))
                .Join(points[3].transform.DOLocalMove(defaultCablePositions[3], 2f))
                .Join(points[4].transform.DOLocalMove(defaultCablePositions[4], 2f));

            sequence.OnUpdate(() =>
            {
                _cableBezierCurveRenderer.DrawCurvedTubeLine(tubeRenderer,
                    points[0].localPosition, points[1].localPosition,
                    points[2].localPosition, points[3].localPosition,
                    points[4].localPosition);
            });

            return sequence;
        }

        /// <summary>
        /// Metóda vytvára mapu kde klúčmi sú jednotlivé objekty káblov a hodnotou je tuple
        /// obsahujúci počiatočné rozmiestnenie bodov kábla a pomer vzdialenosti medzi max výškou letu a
        /// bočnou vzdialenosťou akú môže kabel maximálne vykonať pri vzlete
        /// (MaxHeight / 100) - deleno 100 kvoli tomu ze mierka je [0,1] cize max vyska vzletu je 0.8 nie 80
        /// </summary>
        private Dictionary<GameObject, (Vector3[] localPositions, float sideMovementDistance)>
            GetDefaultCableParameters(IEnumerable<GameObject> cables)
        {
            var parameters = new Dictionary<GameObject, (Vector3[], float)>();

            foreach (var cable in cables)
            {
                var localPositions = cable.transform.GetComponentsInChildren<Transform>()
                    .Where(go => go.gameObject != cable)
                    .Select(t => t.localPosition).ToArray();

                parameters[cable] = (localPositions,
                    Vector3.Distance(localPositions[0], localPositions[1]) * (5f / 6f) / (MaxHeight / 100));
            }

            return parameters;
        }

        private void ResetPropellerRotation()
        {
            _rotationSpeed = MaxRotationSpeed;
            _slowDown = false;
            foreach (var propeller in _propellers)
            {
                propeller.transform.eulerAngles = new Vector3(-90, 0, 0);
            }
            
            _enablePropellerRotation = true;
        }
    }
}