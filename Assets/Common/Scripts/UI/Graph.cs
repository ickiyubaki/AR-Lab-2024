using System;
using System.Collections.Generic;
using System.Linq;
using Common.Scripts.Utils;
using Localization.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Scripts.UI
{
    public readonly struct GraphData
    {
        public LocalizationKeyValue LineLabel { get; }
        public Color LineColor { get; }
        public List<float> Points { get; }

        public GraphData(LocalizationKeyValue lineLabel, Color lineColor, List<float> points)
        {
            LineLabel = lineLabel;
            LineColor = lineColor;
            Points = points;
        }

        public (LocalizationKeyValue lineName, Color lineColor) GetGraphLegend() => (LineLabel, LineColor);
        public (List<float> points, Color lineColor) GetGraphPoints() => (Points, LineColor);
    }

    public class Graph : Singleton<Graph>
    {
        private const int YSeparatorCount = 5;
        private const int XSeparatorCount = 8;
        private const int MaxNumberOfPoint = 120;

        [SerializeField] private Sprite circleSprite;
        [SerializeField] private RectTransform graphContainer;
        [SerializeField] private GameObject legendPrefab;
        [SerializeField] private TextMeshProUGUI graphTitle;

        private PoolManager _poolManager;

        private Dictionary<Color, bool> _lineToggles;
        private List<GameObject> _legendObjectList;
        private List<GameObject> _graphObjectList;
        private List<GameObject> _pooledObjectList;

        private RectTransform _labelTemplateX;
        private RectTransform _dataCanvas;
        private RectTransform _labelTemplateY;
        private RectTransform _dashTemplateX;
        private RectTransform _dashTemplateY;
        private RectTransform _connection;
        private RectTransform _legend;

        private List<GraphData> _graphDataLines;
        private decimal _xStep;
        private string _xLabel;
        private string _yLabel;

        private bool _newSetUp;

        public override void Awake()
        {
            base.Awake();

            _dataCanvas = graphContainer.Find("DataCanvas").GetComponent<RectTransform>();
            _labelTemplateX = _dataCanvas.Find("LabelTemplateX").GetComponent<RectTransform>();
            _labelTemplateY = _dataCanvas.Find("LabelTemplateY").GetComponent<RectTransform>();
            _dashTemplateX = _dataCanvas.Find("DashTemplateX").GetComponent<RectTransform>();
            _dashTemplateY = _dataCanvas.Find("DashTemplateY").GetComponent<RectTransform>();
            _connection = _dataCanvas.Find("Connection").GetComponent<RectTransform>();
            _legend = graphContainer.Find("Legend").GetComponent<RectTransform>();
            _graphDataLines = new List<GraphData>();
            _graphObjectList = new List<GameObject>();
            _pooledObjectList = new List<GameObject>();
            _legendObjectList = new List<GameObject>();
            _lineToggles = new Dictionary<Color, bool>();

            _poolManager = PoolManager.Instance;
            _poolManager.CreatePool(_connection.gameObject, 309);
            _poolManager.CreatePool(_labelTemplateX.gameObject, XSeparatorCount + 1);
            _poolManager.CreatePool(_dashTemplateX.gameObject, XSeparatorCount + 1);
            _poolManager.CreatePool(_labelTemplateY.gameObject, YSeparatorCount + 1);
            _poolManager.CreatePool(_dashTemplateY.gameObject, YSeparatorCount + 2);
        }

        public void SetUpGraph(LocalizationKeyValue title, string xLabel, string yLabel)
        {
            _newSetUp = true;
            DestroyGraph(_legendObjectList);
            graphTitle.gameObject.GetComponent<UILocalizationEditable>().SetKeyAndValue(title);
            _xLabel = xLabel;
            _yLabel = yLabel;
            _xStep = 0;
            _graphDataLines.Clear();
            _lineToggles.Clear();
        }

        public void DrawGraph(List<GraphData> graphDataLines, decimal xStep)
        {
            if (_newSetUp)
            {
                CreateLegend(graphDataLines.Select(dl => dl.GetGraphLegend()).ToList());
                _newSetUp = false;
            }

            DestroyGraph(_graphObjectList);
            _graphObjectList.Clear();
            _poolManager.Return(_pooledObjectList);
            _pooledObjectList.Clear();
            _graphDataLines = graphDataLines;
            _xStep = xStep;

            // vytvorenie kópie dat, aby sme nemodilikovali vstupný parameter
            var activeDataLines =
                new List<(List<float> points, Color lineColor)>(_graphDataLines.Select(dl =>
                    (new List<float>(dl.Points), dl.LineColor)));
            var numberOfRecords = activeDataLines.Select(dl => dl.points).Max(p => p.Count);
            var maxLabelNumber = Convert.ToDecimal(numberOfRecords) * xStep;
            var minLabelNumber = 0m;

            if (numberOfRecords > MaxNumberOfPoint)
            {
                var difference = numberOfRecords - MaxNumberOfPoint;
                foreach (var points in activeDataLines.Select(dl => dl.points))
                {
                    points.RemoveRange(0, difference);
                }

                minLabelNumber = difference * _xStep;
                numberOfRecords = MaxNumberOfPoint;
            }

            foreach (var toggle in _lineToggles.Where(toggle => !toggle.Value))
            {
                activeDataLines.Remove(activeDataLines.Single(data => data.lineColor == toggle.Key));
            }

            var graphWidth = graphContainer.rect.width;
            var graphHeight = graphContainer.sizeDelta.y;
            var xSize = graphWidth / numberOfRecords;

            // Find min|max value
            var yMaximum = activeDataLines.Select(dl => dl.points.Max()).Prepend(0f).Max();
            yMaximum = yMaximum >= 0 ? yMaximum * 1.1f : yMaximum / 1.1f;
            var yMinimum = activeDataLines.Select(dl => dl.points.Min()).Prepend(0f).Min();
            yMinimum = yMinimum >= 0 ? yMinimum / 1.1f : yMinimum * 1.1f;

            var lastPoints = new Vector2[activeDataLines.Count];
            for (var i = 0; i < numberOfRecords; i++)
            {
                var xPosition = xSize + i * xSize;
                CreatePointsAndConnections(activeDataLines, lastPoints, i, xPosition, graphHeight, yMaximum, yMinimum);
            }

            CreateXLabels(d => $"{((maxLabelNumber - minLabelNumber) * d + minLabelNumber):0.#}" + _xLabel, graphWidth);
            CreateYLabels(f => Mathf.RoundToInt(f) + _yLabel, graphHeight, yMaximum, yMinimum);
        }

        private void CreatePointsAndConnections(IEnumerable<(List<float> points, Color lineColor)> dataLines,
            IList<Vector2> lastPoints, int pointIndex, float xPosition, float graphHeight,
            float yMaximum, float yMinimum)
        {
            foreach (var ((points, lineColor), valueIndex) in dataLines.Select((dataLine, valueIndex) =>
                (dataLine, valueIndex)))
            {
                // min-max normalization
                var yPosition = (points[pointIndex] - yMinimum) / (yMaximum - yMinimum) * graphHeight;
                var pointPosition = new Vector2(xPosition, yPosition);
                var lastPointPosition = lastPoints.ElementAtOrDefault(valueIndex);

                // uncomment to show point as circle
                // CreatePoint(new Vector2(xPosition, yPosition));

                if (pointIndex > 0)
                {
                    CreateConnection(lineColor, lastPointPosition, pointPosition);
                }

                lastPoints[valueIndex] = pointPosition;
            }
        }

        private GameObject CreatePoint(Vector2 anchoredPosition)
        {
            var point = new GameObject("point", typeof(Image));
            point.transform.SetParent(_dataCanvas, false);
            point.GetComponent<Image>().sprite = circleSprite;

            var rectTransform = point.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(20, 20);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);

            _graphObjectList.Add(point);
            return point;
        }

        private GameObject CreateConnection(Color lineColor, Vector2 pointPositionA, Vector2 pointPositionB)
        {
            var connection = _poolManager.Get(_connection.gameObject);
            var connectionTransform = connection.GetComponent<RectTransform>();
            connectionTransform.SetParent(_dataCanvas, false);
            connectionTransform.GetComponent<RawImage>().color = lineColor;
            var dir = (pointPositionB - pointPositionA).normalized;
            var distance = Vector2.Distance(pointPositionA, pointPositionB);
            connectionTransform.anchorMin = new Vector2(0, 0);
            connectionTransform.anchorMax = new Vector2(0, 0);
            connectionTransform.sizeDelta = new Vector2(distance, 5f);
            connectionTransform.anchoredPosition = pointPositionA + dir * distance * 0.5f;
            connectionTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));

            _pooledObjectList.Add(connection);

            return connection;
        }

        private void CreateXLabels(Func<decimal, string> getAxisLabelX, float graphWidth)
        {
            for (var i = 0; i <= XSeparatorCount; i++)
            {
                var labelX = _poolManager.Get(_labelTemplateX.gameObject);
                var labelXTransform = labelX.GetComponent<RectTransform>();
                labelXTransform.SetParent(_dataCanvas, false);
                var normalizedValue = i * 1f / XSeparatorCount;
                labelXTransform.anchoredPosition = new Vector2(normalizedValue * graphWidth, -20f);
                labelXTransform.GetComponent<TextMeshProUGUI>().text =
                    getAxisLabelX(Convert.ToDecimal(normalizedValue));
                _pooledObjectList.Add(labelX);

                var dashX = _poolManager.Get(_dashTemplateX.gameObject);
                var dashXTransform = dashX.GetComponent<RectTransform>();
                dashXTransform.SetParent(_dataCanvas, false);
                dashXTransform.anchoredPosition = new Vector2(normalizedValue * graphWidth, -3f);
                _pooledObjectList.Add(dashX);
            }
        }

        private void CreateYLabels(Func<float, string> getAxisLabelY, float graphHeight, float yMaximum, float yMinimum)
        {
            for (var i = 0; i <= YSeparatorCount; i++)
            {
                var labelY = _poolManager.Get(_labelTemplateY.gameObject);
                var labelYTransform = labelY.GetComponent<RectTransform>();
                labelYTransform.SetParent(_dataCanvas, false);
                var normalizedValue = i * 1f / YSeparatorCount;
                labelYTransform.anchoredPosition = new Vector2(-30f, normalizedValue * graphHeight);
                labelYTransform.GetComponent<TextMeshProUGUI>().text =
                    getAxisLabelY((yMaximum - yMinimum) * normalizedValue + yMinimum);
                _pooledObjectList.Add(labelY);

                var dashY = _poolManager.Get(_dashTemplateY.gameObject);
                var dashYTransform = dashY.GetComponent<RectTransform>();
                dashYTransform.SetParent(_dataCanvas, false);
                dashYTransform.anchoredPosition = new Vector2(-3f, normalizedValue * graphHeight);
                _pooledObjectList.Add(dashY);
            }

            if (yMinimum != 0)
            {
                var dashY0 = _poolManager.Get(_dashTemplateY.gameObject);
                var dashY0Transform = dashY0.GetComponent<RectTransform>();
                dashY0Transform.SetParent(_dataCanvas, false);
                dashY0.GetComponent<Image>().color = Color.gray;
                dashY0Transform.anchoredPosition = new Vector2(-3f, Mathf.Abs(yMinimum));
                _pooledObjectList.Add(dashY0);
            }
        }

        private void CreateLegend(
            IEnumerable<(LocalizationKeyValue lineName, Color lineColor)> lineLegends)
        {
            foreach (var (lineName, lineColor) in lineLegends)
            {
                var legend = Instantiate(legendPrefab, _legend, false);
                var legendButton = legend.GetComponentInChildren<Button>();
                legendButton.onClick.AddListener(delegate { ToggleLine(lineColor); });
                foreach (var image in legendButton.GetComponentsInChildren<Image>())
                {
                    if (image.gameObject.CompareTag("graph_legend_color"))
                    {
                        image.color = lineColor;
                        break;
                    }
                }

                legend.GetComponentInChildren<UILocalizationEditable>().SetKeyAndValue(lineName);
                _legendObjectList.Add(legend);
                _lineToggles.Add(lineColor, true);
            }
        }

        private void ToggleLine(Color lineColor)
        {
            _lineToggles[lineColor] = !_lineToggles[lineColor];
            DrawGraph(_graphDataLines, _xStep);
        }

        private void DestroyGraph(List<GameObject> gameObjects)
        {
            foreach (var go in gameObjects)
            {
                Destroy(go);
            }
        }

        public void ResetGraph()
        {
            _poolManager.Return(_pooledObjectList);
            _pooledObjectList.Clear();
            DestroyGraph(_graphObjectList);
            _graphObjectList.Clear();
            DestroyGraph(_legendObjectList);
            _legendObjectList.Clear();
            _graphDataLines.Clear();
            graphTitle.text = "";
        }

        /// <summary>
        /// Metóda vráti uhol otočenia (0-360) z vektoru
        /// </summary>
        private float GetAngleFromVectorFloat(Vector3 dir)
        {
            dir = dir.normalized;
            var n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (n < 0) n += 360;

            return n;
        }
    }
}