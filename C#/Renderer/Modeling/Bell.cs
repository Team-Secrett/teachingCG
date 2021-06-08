using Renderer.Modeling;
using Rendering;
using System;
using System.Collections.Generic;
using static GMath.Gfx;
using System.Linq;
using System.Text;
using GMath;
using System.Drawing;
using static Renderer.Program;
using Renderer;
using System.IO;

namespace Renderer
{
    public static class MyMath
    {
        public static float GaussBell(float x, float a, float b, float c)
        {
            var v1 = (x - b);
            var v2 = (v1 * v1) / (2 * (c * c));
            var v3 = (a * Math.Exp(-v2));

            return (float)v3;
        }
    }
    public static class Bell<T> where T : struct, IVertex<T>, INormalVertex<T>, ICoordinatesVertex<T>
    {
        public static float RingHeight = 1.2f;
        public static float UpperHeight = 4;
        public static float BellHeight = 4;

        public static Mesh<T> GaussBellMesh()
        {

            int points = 20;
            float3[] contourn = new float3[points];
            int id = 0;
            float width = 0.8f;
            float delta = (1f * width) / points;
            for (float i = 0;i < width; i += delta)
            {
                contourn[id++] = float3(i, MyMath.GaussBell(i, 1, 0.19f, 0.2f), 0f);
            }

            var model = Manifold<T>.Revolution(20, 30, t => MeshShapeGenerator<T>.EvalBezier(contourn, t), float3(0, 1, 0)).Weld();
            return model;
        }

        public static Mesh<T> RingMesh()
        {
            float[] handleRing =
            {
                0.3f,
                0.6f,
                1,
                0.8f,
                0.35f
            };
            float3[] contourn = Bell<T>.travel(handleRing, Bell<T>.RingHeight);

            var model = Manifold<T>.Revolution(20, 30, t => MeshShapeGenerator<T>.EvalBezier(contourn, t), float3(0, 1, 0)).Weld();
            return model;
        }

        public static Mesh<T> HandleUpperMesh()
        {
            float[] handleUpper =
            {
                0.35f,
                0.35f,
                0.4f,
                0.5f,
                0.8f,
                // 0.7f,
                // 0.8f,
                0.8f,
                0f,
            };
            float3[] contourn = Bell<T>.travel(handleUpper, Bell<T>.UpperHeight);
            var model = Manifold<T>.Revolution(20, 30, t => MeshShapeGenerator<T>.EvalBezier(contourn, t), float3(0, 1, 0)).Weld();
            model.ComputeNormals();
            return model;
        }

        public static Mesh<T> BellMesh()
        {
            var upperMesh = Bell<T>.HandleUpperMesh();
            var ringMesh = Bell<T>.RingMesh();
            upperMesh = upperMesh.Transform(Transforms.Translate(0, Bell<T>.RingHeight, 0));
            upperMesh.ComputeNormals();

            var handleMesh = upperMesh + ringMesh;
            handleMesh = handleMesh.Transform(Transforms.Scale(0.2f, 0.2f, 0.2f));
            handleMesh = handleMesh.Transform(Transforms.Translate(0, 0.8f, 0));
            handleMesh.ComputeNormals();

            var bellMesh = Bell<T>.GaussBellMesh();
            bellMesh.ComputeNormals(true);
            var finalMesh = (handleMesh + bellMesh);

            finalMesh.Weld();

            return finalMesh;
        }

        public static void AddToScene(Scene<T, Material> scene, float4x4 transform)
        {
            var bellMesh = Bell<T>.BellMesh();
            var material = new Material
            {
                Specular = float3(1, 1, 1) * 0.6f,
                SpecularPower = 260,
                Diffuse = float3(4.3f, 4.3f, 4.3f),

                WeightDiffuse = 1,
                WeightMirror = 1f,
                RefractionIndex = 0.02f,
            };
            bellMesh = bellMesh.Transform(Transforms.Scale(1.5f, 1.7f, 1.5f));

            scene.Add(bellMesh.AsRaycast(RaycastingMeshMode.Grid), material, transform);
        }

        public static float3[] travel(float[] arr, float len, float start = 0)
        {
            int n = arr.Length;
            float3[] ret = new float3[n];
            float step = start;
            float delta = (1.0f * len) / (n - 1);

            for (int i = 0; i < n; i++)
            {
                ret[i] = float3(arr[i], step, 0);
                step += delta;
            }
            return ret;
        }
    }
}
