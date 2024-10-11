using System;
using UnityEngine;


namespace Pincushion
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class StaticSampledPointsGenerator : MonoBehaviour
    {
        public float pointsPerM = 0.015f;
        public float pointSize = 0.02f;
        public Color color = new Color(0.9f, 0.9f, 0.9f);
        public StaticPointsCreationMode mode = StaticPointsCreationMode.replace;


        private void Awake()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            // Sample the points.
            Vector3[] points = meshFilter.mesh.GetSampledPoints(pointsPerM);
            
            // Create the material.
            Material material = new Material(Shader.Find("Pincushion/StaticPoints"));
            // Set material values.
            material.SetFloat("_PointSize", pointSize);
            material.SetColor("_Color", color);
            
            // Decide what to do with the material and points.
            if (mode == StaticPointsCreationMode.create)
            {
                Create(points, material);
            }
            else if (mode == StaticPointsCreationMode.replace)
            {
                meshFilter.mesh.vertices = points;
                GetComponent<MeshRenderer>().material = material;
            }
            else if (mode == StaticPointsCreationMode.createAndHideOriginal)
            {
                Create(points, material).SetOriginalVisibility(false);
            }
            else
            {
                throw new Exception("Invalid mode: " + mode);
            }
        }


        private StaticPointsRenderer Create(Vector3[] points, Material material)
        {
            // Create a new object.
            GameObject go = new GameObject();
            // Match my transform.
            Transform t = transform;
            go.transform.position = t.position;
            go.transform.rotation = t.rotation;
            go.transform.localScale = t.localScale;
            // Add the mesh and the renderer.
            Mesh sampledMesh = new Mesh();
            sampledMesh.vertices = points;
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = sampledMesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            // Set the material.
            mr.material = material;
            // Render.
            StaticPointsRenderer staticPointsRenderer = go.AddComponent<StaticPointsRenderer>();
            staticPointsRenderer.o = gameObject;
            staticPointsRenderer.r = mr;
            // Parent myself.
            t.parent = go.transform;
            return staticPointsRenderer;
        }
    }
}