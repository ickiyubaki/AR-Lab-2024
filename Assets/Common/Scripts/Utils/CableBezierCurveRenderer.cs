using System.Collections.Generic;
using UnityEngine;

namespace Common.Scripts.Utils
{
    //[ExecuteInEditMode]
    public class CableBezierCurveRenderer //: MonoBehaviour
    {
        // public Transform point1;
        // public Transform point2;
        // public Transform point3;
        // public Transform point4;
        // public Transform point5;
        // public TubeRenderer tubeRenderer;

        // private void Update()
        // {
        //     DrawCurvedTubeLine(tubeRenderer, point1.localPosition, point2.localPosition, point3.localPosition,
        //         point4.localPosition, point5.localPosition);
        // }

        private const int Smoothness = 26;

        public void DrawCurvedTubeLine(TubeRenderer tubeRenderer, Vector3 a, Vector3 b, Vector3 c, Vector3 d,
            Vector3 e)
        {
            var bezierPoints = new List<Vector3>();

            for (float ration = 0; ration <= 1; ration += 1.0f / Smoothness)
            {
                bezierPoints.Add(QuarticLerp(a, b, c, d, e, ration));
            }

            tubeRenderer.SetPositions(bezierPoints.ToArray());
        }

        // linear interpolation
        private Vector3 LinearLerp(Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);

        // quadratic interpolation
        private Vector3 QuadraticLerp(Vector3 a, Vector3 b, Vector3 c, float t) =>
            Vector3.Lerp(LinearLerp(a, b, t), LinearLerp(b, c, t), t);

        // cubic interpolation
        private Vector3 CubicLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) =>
            Vector3.Lerp(QuadraticLerp(a, b, c, t), QuadraticLerp(b, c, d, t), t);

        // quartic interpolation
        private Vector3 QuarticLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e, float t) =>
            Vector3.Lerp(CubicLerp(a, b, c, d, t), CubicLerp(b, c, d, e, t), t);
    }
}