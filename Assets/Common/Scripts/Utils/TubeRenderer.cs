// Author: Mathias Soeholm
// Date: 05/10/2016

using UnityEngine;

namespace Common.Scripts.Utils
{
    [ExecuteInEditMode]
    public class TubeRenderer : MonoBehaviour
    {
        [SerializeField] 
        private Vector3[] positions;
        [SerializeField] 
        private int sides;
        [SerializeField] 
        private float radiusOne;
        [SerializeField] 
        private float radiusTwo;
        [SerializeField] 
        private bool useWorldSpace = true;
        [SerializeField] 
        private bool useTwoRadii;

        private Vector3[] _vertices;
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        public Material Material
        {
            get => _meshRenderer.material;
            set => _meshRenderer.material = value;
        }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter == null)
            {
                _meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshRenderer == null)
            {
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            _mesh = new Mesh();
            _meshFilter.mesh = _mesh;
        }

        private void OnEnable()
        {
            _meshRenderer.enabled = true;
        }

        private void OnDisable()
        {
            _meshRenderer.enabled = false;
        }

        private void Update()
        {
            GenerateMesh();
        }

        private void OnValidate()
        {
            sides = Mathf.Max(3, sides);
        }

        public void SetPositions(Vector3[] positionsVector3)
        {
            this.positions = positionsVector3;
            GenerateMesh();
        }

        private void GenerateMesh()
        {
            if (_mesh == null || positions == null || positions.Length <= 1)
            {
                _mesh = new Mesh();
                return;
            }

            var verticesLength = sides * positions.Length;
            if (_vertices == null || _vertices.Length != verticesLength)
            {
                _vertices = new Vector3[verticesLength];

                var indices = GenerateIndices();
                var uvs = GenerateUVs();

                if (verticesLength > _mesh.vertexCount)
                {
                    _mesh.vertices = _vertices;
                    _mesh.triangles = indices;
                    _mesh.uv = uvs;
                }
                else
                {
                    _mesh.triangles = indices;
                    _mesh.vertices = _vertices;
                    _mesh.uv = uvs;
                }
            }

            var currentVertIndex = 0;

            for (int i = 0; i < positions.Length; i++)
            {
                var circle = CalculateCircle(i);
                foreach (var vertex in circle)
                {
                    _vertices[currentVertIndex++] = useWorldSpace ? transform.InverseTransformPoint(vertex) : vertex;
                }
            }

            _mesh.vertices = _vertices;
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            _meshFilter.mesh = _mesh;
        }

        private Vector2[] GenerateUVs()
        {
            var uvs = new Vector2[positions.Length * sides];

            for (int segment = 0; segment < positions.Length; segment++)
            {
                for (int side = 0; side < sides; side++)
                {
                    var vertIndex = (segment * sides + side);
                    var u = side / (sides - 1f);
                    var v = segment / (positions.Length - 1f);

                    uvs[vertIndex] = new Vector2(u, v);
                }
            }

            return uvs;
        }

        private int[] GenerateIndices()
        {
            // Two triangles and 3 vertices
            var indices = new int[positions.Length * sides * 2 * 3];

            var currentIndicesIndex = 0;
            for (int segment = 1; segment < positions.Length; segment++)
            {
                for (int side = 0; side < sides; side++)
                {
                    var vertIndex = (segment * sides + side);
                    var prevVertIndex = vertIndex - sides;

                    // Triangle one
                    indices[currentIndicesIndex++] = prevVertIndex;
                    indices[currentIndicesIndex++] = (side == sides - 1) ? (vertIndex - (sides - 1)) : (vertIndex + 1);
                    indices[currentIndicesIndex++] = vertIndex;


                    // Triangle two
                    indices[currentIndicesIndex++] =
                        (side == sides - 1) ? (prevVertIndex - (sides - 1)) : (prevVertIndex + 1);
                    indices[currentIndicesIndex++] = (side == sides - 1) ? (vertIndex - (sides - 1)) : (vertIndex + 1);
                    indices[currentIndicesIndex++] = prevVertIndex;
                }
            }

            return indices;
        }

        private Vector3[] CalculateCircle(int index)
        {
            var dirCount = 0;
            var forward = Vector3.zero;

            // If not first index
            if (index > 0)
            {
                forward += (positions[index] - positions[index - 1]).normalized;
                dirCount++;
            }

            // If not last index
            if (index < positions.Length - 1)
            {
                forward += (positions[index + 1] - positions[index]).normalized;
                dirCount++;
            }

            // Forward is the average of the connecting edges directions
            forward = (forward / dirCount).normalized;
            var side = Vector3.Cross(forward, forward + new Vector3(.123564f, .34675f, .756892f)).normalized;
            var up = Vector3.Cross(forward, side).normalized;

            var circle = new Vector3[sides];
            var angle = 0f;
            var angleStep = (2 * Mathf.PI) / sides;

            var t = index / (positions.Length - 1f);
            var radius = useTwoRadii ? Mathf.Lerp(radiusOne, radiusTwo, t) : radiusOne;

            for (int i = 0; i < sides; i++)
            {
                var x = Mathf.Cos(angle);
                var y = Mathf.Sin(angle);

                circle[i] = positions[index] + side * (x * radius) + up * (y * radius);

                angle += angleStep;
            }

            return circle;
        }
    }
}