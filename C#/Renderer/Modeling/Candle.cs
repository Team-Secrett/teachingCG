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
    public static class Candle<T> where T : struct, IVertex<T>, INormalVertex<T>, ICoordinatesVertex<T>
    {
        public static float GlassHeight = 2.5f;

        public static float GlassUpperRadius = 1f;

        public static float GlassLowerRadius = 0.85f;


        /// Glass width radius in body
        public static float UpperGlassRadius = 0.1f;

        /// Glass width radius in bottom
        public static float BottomGlassRadius = 0.17f;

        public static float GlassBottomHeight = 0.5f;

        public static float GlassBottomLowerRadius = .85f;

        public static float StickHeight = 0.7f;

        public static void AddToScene(Scene<T, Material> scene, float4x4 transform)
        {
            /// bottom of the glass
            var glassBottom = GlassBottom().Transform(Transforms.Translate(0, 0.07f, 0));
            Material glassBottomMaterial = new Material
            {
                Specular = float3(1, 1, 1),
                SpecularPower = 60,
                Diffuse = float3(1, 1, 1),

                WeightDiffuse = 1f,
                WeightFresnel = 1f, 
                RefractionIndex = 1.15f
            };

            /// glass body
            var glassBody = GlassBody();
            Material glassBodyMaterial = new Material
            {
                Specular = float3(1, 1, 1) * 0.65f,
                SpecularPower = 260,
                Diffuse = float3(1, 1, 1),

                WeightDiffuse = 0,
                WeightFresnel = 1.5f,
                RefractionIndex = .96f,
            };
            glassBody = glassBody.Transform(Transforms.Translate(0, GlassBottomHeight, 0));

            /// wax
            var wax = WaxMesh();
            wax = wax.Transform(Transforms.Translate(0, GlassBottomHeight, 0));

            Material waxMaterial = new Material
            {
                Specular = float3(1, 1, 1) * 0.2f,
                SpecularPower = 60,
                // Diffuse = float3(0.24f, 0.24f, 0.24f),
                Diffuse = float3(0.8f, 0.55f, 0.14f),

                Emissive = 0.2f,
                // WeightMirror = 2,
                // WeightFresnel = 2,
                // RefractionIndex = 2,
            };

            // stick
            var stick = BurnedStick();
            stick = stick.Transform(Transforms.Translate(0, 1.1f, 0));

            var stickMaterial = new Material
            {
                Specular = float3(1, 1, 1) * 0.1f,
                SpecularPower = 60,
                Diffuse = float3(0.1f, 0.1f, 0.1f),
            };


            // fire
            var fire = FireMesh();
            fire = fire.Transform(Transforms.Translate(-0.02f, 1 + StickHeight, 0.36f));

            var fireMaterial = new Material
            {
                Specular = float3(1, 1, 1) * 0.1f,
                SpecularPower = 260,
                Diffuse = float3(0.74f, 0.44f, 0.14f),
                Emissive =  170 / (4 * pi),
            };

            scene.Add(glassBottom.AsRaycast(RaycastingMeshMode.Grid), glassBottomMaterial, transform);
            scene.Add(glassBody.AsRaycast(RaycastingMeshMode.Grid), glassBodyMaterial, transform);
            scene.Add(wax.AsRaycast(RaycastingMeshMode.Grid), waxMaterial, transform);
            scene.Add(stick.AsRaycast(RaycastingMeshMode.Grid), stickMaterial, transform);
            scene.Add(fire.AsRaycast(RaycastingMeshMode.Grid), fireMaterial, transform);
        }

        public static Mesh<T> GlassBody()
        {
            float3[] contourn =
            {
                float3(GlassLowerRadius, 0, 0),
                float3(GlassUpperRadius, GlassHeight, 0),

                float3(GlassLowerRadius - UpperGlassRadius, GlassHeight, 0),
                float3(GlassLowerRadius - UpperGlassRadius, 0, 0)
            };

            var model = Manifold<T>.Revolution(20, 30, t => MeshShapeGenerator<T>.EvalBezier(contourn, t), float3(0, 1, 0)).Weld();
            model.ComputeNormals(true);
            
            return model.Weld();
        }

        public static Mesh<T> HandleMesh()
        {
            var circle = MeshShapeGenerator<T>.Circle(2);

            var model = Manifold<T>.Generative(40, 50, u => float3(3 * cos(u), 0, 3 * sin(u)), (x, t) => t * t * x.x);
            // model = model.Transform(Transforms.Rotate(90, float3(0, 1, 0)));
            model.ComputeNormals();

            return model;
        }

        public static Mesh<T> GlassBottom()
        {
            float3[] contourn =
            {
                float3(GlassBottomLowerRadius, 0, 0),
                float3(GlassLowerRadius, GlassBottomHeight, 0),

                // float3(GlassLowerRadius - BottomGlassRadius, GlassBottomHeight, 0),
                // float3(GlassLowerRadius - BottomGlassRadius, 0, 0)
            };

            var model = Manifold<T>.Revolution(20, 30, t => MeshShapeGenerator<T>.EvalBezier(contourn, t), float3(0, 1, 0)).Weld();
            model.ComputeNormals(true);

            var circle = MeshShapeGenerator<T>.Circle(GlassBottomLowerRadius);

            return (model + circle).Weld();
        }

        public static Mesh<T> WaxMesh()
        {
            // same as glassbody
            float scale = 0.92f;
            float upperScale = 0.5f;
            var model = GlassBody();
            model = model.Transform(Transforms.Scale(scale, upperScale, scale)).Weld();
            model = model.Transform(Transforms.Translate(0, 0, 0)).Weld();
            model.ComputeNormals(true);
            var model2 = model.Clone().Weld();
            model2.ComputeNormals();

            float3[] topContourn =
            {
                float3(0, 0, 0),
                float3(GlassUpperRadius * scale - 0.1f, 0, 0)
            };
            float3[] bottomContourn =
            {
                float3(0, 0, 0),
                float3(GlassLowerRadius * scale - 0.1f, 0, 0)
            };
            var topCircle = Manifold<T>.Revolution(20, 30, t => MeshShapeGenerator<T>.EvalBezier(topContourn, t), float3(0, 1, 0)).Weld();
            var topCircle2 = topCircle.Clone();
            topCircle.ComputeNormals(true);
            topCircle2.ComputeNormals();

            topCircle = topCircle.Transform(Transforms.Translate(0, 1.72f * upperScale, 0));

            var bottomCircle = Manifold<T>.Revolution(20, 30, t => MeshShapeGenerator<T>.EvalBezier(topContourn, t), float3(0, 1, 0)).Weld();
            var bottomCircle2 = bottomCircle.Clone();
            bottomCircle.ComputeNormals(true);
            bottomCircle.ComputeNormals();

            return (topCircle + topCircle2 + bottomCircle + bottomCircle2 + model + model2).Weld();
        }

        public static Mesh<T> BurnedStick()
        {
            float3[] contourn =
            {
                float3(0, 0, 0),
                float3(0.05f, 0, 0),
                float3(0.05f, StickHeight, 0),
                float3(0f, StickHeight, 0),
            };

            var model = Manifold<T>.Revolution(20, 30, t => MeshShapeGenerator<T>.EvalBezier(contourn, t), float3(0, 1, 0)).Weld();
            model = model.Transform(Transforms.Rotate(0.3f, float3(0, 0, 1)));
            model.ComputeNormals();
            return model;
        }
        
        public static Mesh<T> LidMesh()
        {
            return MeshShapeGenerator<T>.Circle(0.412f);
        }

        public static Mesh<T> FireMesh()
        {
            float3[] contourn =
            {
                float3(0, 0, 0),
                float3(-0.02f, 0, 0),
                float3(-0.04f, 0.1f, 0),
                float3(-0.055f, 0.15f, 0),
                float3(-0.055f, 0.19f, 0),
                float3(-0.045f, 0.2f, 0),
                float3(-0.035f, 0.27f, 0),
                float3(-0.025f, 0.34f, 0),
                float3(-0.015f, 0.5f, 0),
            };

            var model = Manifold<T>.Revolution(20, 30, t => MeshShapeGenerator<T>.EvalBezier(contourn, t), float3(0, 1, 0)).Weld();
            model = model.Transform(Transforms.Scale(2.8f, 1.8f, 2.8f));
            model.ComputeNormals(true);

            return model;
        }
    }
}