#region
//
// Author: Lukas Gregori
// Date: 16.07.2015
//
// Simple Gerstner Wave Script. Ideas and math from
// graphics.ucsd.edu/courses/rendering/2005/jdewall/tessendorf.pdf
//
// The limit of waves that can be produced is capped at 100. This
// can be changed (if needed) by increasing the MaxWaveCount constant.
//
// Filename: GerstnerWavesScript.cs
//
#endregion

using System;

namespace AssemblyCSharp
{
    using UnityEngine;
    using System.Collections;
    public class GerstnerWavesScript : MonoBehaviour
    {
        private const int MaxWaveCount = 100;
        public int numberOfWaves = 10;

        public float speed = 0.4f;

        private Vector3[] initialVertices;

        // Wave Amplitude (Min <= Ai <= Max)
        public float maxAmplitude = 0.02f;
        private float maxAmplitudeOld = 0.02f;
        public float minAmplitude = 0.001f;
        private float minAmplitudeOld = 0.001f;

        private float[] Ai = new float[MaxWaveCount];

        // Length of the waves
        public float maxLambda = 8.0f;
        private float maxLambdaOld = 8.0f;
        private float[] lambda = new float[MaxWaveCount];

        // Magnitude
        private float[] ki = new float[MaxWaveCount];

        // Wave vector -> Point where the waves will move towards
        // Given this point we approximate 100 slightly off directions
        // (looks more natural)
        public Vector2 targetPoint = new Vector2(1.0f, 1.0f);
        private Vector2 targetPointOld = new Vector2(1.0f, 1.0f);
        private Vector2[] directions = new Vector2[MaxWaveCount];

        // Set to true to use random directions
        public bool useRandomDirections = true;
        private bool useRandomDirectionsOld = true;

        // Gravitational Constant
        private float g = 9.81f;

        private float[] frequencies = new float[MaxWaveCount];
        public float waterDepth = 10.0f;
        private float waterDepthOld = 10.0f;

        public GerstnerWavesScript()
        {
            Console.Write("Start Gerstner Wave Script.");
        }

        private void Start()
        {
            // Initialize Parameters
            for (var i = 0; i < MaxWaveCount; i++)
            {
                lambda[i] = Random.Range(2.0f, maxLambda);
                ki[i] = (float)(2.0f * Math.PI / lambda[i]);

                frequencies[i] = (float)(Math.Sqrt(g * ki[i] * Math.Tanh(ki[i] * waterDepth)));

                Ai[i] = Random.Range(minAmplitude, maxAmplitude);

                directions[i] = (useRandomDirections) ?
                        new Vector2(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f)) :
                        GenerateTargetDirections();
            }
        }

        /// <summary>
        /// First call the HandleChanges Method and check if anything
        /// changes since the last callt to update. Then initialize the mesh-filter and
        /// get the planes (surface) mesh, as well as its vertices.
        /// 
        /// The formulas to calculate the new positions of the vertices was gotten
        /// from graphics.ucsd.edu/courses/rendering/2005/jdewall/tessendorf.pdf
        /// </summary>
        private void Update()
        {
            HandleChanges();

            MeshFilter meshFilter = (MeshFilter)GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh;

            if (mesh == null)
            {
                Debug.Log("Mesh is null");
                mesh = new Mesh();
                Debug.Log(mesh.vertices);
            }

            // Initialize vertices
            if (initialVertices == null)
                initialVertices = mesh.vertices;

            // Update vertices
            Vector3[] newVertices = new Vector3[initialVertices.Length];
            for (int vertCounter = 0; vertCounter < newVertices.Length; vertCounter++)
            {
                Vector3 p_0 = initialVertices[vertCounter];
                Vector2 x_0 = new Vector2(p_0[0], p_0[2]);
                float y_0 = p_0[1];
                float t = Time.time * speed;
                float phi = (float)Math.PI;

                Vector2 x_sum = new Vector2(0.0f, 0.0f);
                float y = 0.0f;
                for (int i = 0; i < numberOfWaves; i++)
                {
                    x_sum += (directions[i] / ki[i]) * Ai[i] * (float)(Math.Sin(Vector2.Dot(directions[i], x_0) - frequencies[i] * t + phi));
                    y += (float)(Ai[i] * Math.Cos(phi * Vector2.Dot(directions[i], x_0) - frequencies[i] * t));
                }

                Vector2 x = x_0 - x_sum;
                Vector3 newVertex = new Vector3(x[0], y, x[1]);
                newVertices[vertCounter] = newVertex;
            }

            mesh.vertices = newVertices;
            mesh.RecalculateNormals();
        }

        /// <summary>
        /// Genereates the target points for the different waves used. 
        /// Every target will be slightly different to the goal, to
        /// ensure a more natural look. Will be between the bounds of the
        /// offset variables.
        /// </summary>
        /// <returns>Point with offset to target</returns>
        private Vector2 GenerateTargetDirections()
        {
            float offset_x = targetPoint.x / 5.0f;
            float offset_y = targetPoint.y / 5.0f;

            return new Vector2(Random.Range(targetPoint.x - offset_x, targetPoint.x + offset_x),
                               Random.Range(targetPoint.y - offset_y, targetPoint.y + offset_y));
        }


        /// <summary>
        /// Method checks if user changed any settings and adapts
        /// the calculation accordingly
        /// </summary>
        private void HandleChanges()
        {
            for (var i = 0; i < MaxWaveCount; i++)
            {
                if (maxLambda != maxLambdaOld ||
                    waterDepth != waterDepthOld)
                {
                    lambda[i] = Random.Range(2.0f, maxLambda);
                    ki[i] = (float)(2.0f * Math.PI / lambda[i]);

                    frequencies[i] = (float)(Math.Sqrt(g * ki[i] * Math.Tanh(ki[i] * waterDepth)));
                }

                if (maxAmplitude != maxAmplitudeOld
                    || minAmplitude != minAmplitudeOld)
                {
                    Ai[i] = Random.Range(minAmplitude, maxAmplitude);
                }

                if (useRandomDirections != useRandomDirectionsOld
                    || targetPoint != targetPointOld)
                {
                    directions[i] = (useRandomDirections) ?
                           new Vector2(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f)) :
                           GenerateTargetDirections();
                }


            }

            // Reset flags
            maxLambdaOld = maxLambda;
            maxAmplitudeOld = maxAmplitude;
            minAmplitudeOld = minAmplitude;
            useRandomDirectionsOld = useRandomDirections;
            targetPointOld = targetPoint;
            waterDepthOld = waterDepth;
        }

    }
}

