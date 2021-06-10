using System;
using System.Collections.Generic;
using System.Text;
using static GMath.Gfx;
using GMath;
using Rendering;
using static Renderer.Program;

namespace Renderer
{
    public static class MeshShapeGenerator<T> where T : struct, IVertex<T>, INormalVertex<T>, ICoordinatesVertex<T>
    {
        public static float3 EvalBezier(float3[] control, float t)
        {
            if (control.Length == 1)
                return control[0];
            float3[] nestedPoints = new float3[control.Length - 1];
            for (int i = 0; i < nestedPoints.Length; i++)
                nestedPoints[i] = lerp(control[i], control[i + 1], t);
            return EvalBezier(nestedPoints, t);
        }

        static Mesh<T> BezierCurves(float3[] control, int slices = 30, int stacks = 30)
        {
            return Manifold<T>.Revolution(slices, stacks, t => EvalBezier(control, t), float3(0, 1, 0));
        }

        public static Mesh<T> Circle(float radius, int slices = 20, int stacks = 30)
        {
            float3[] contourn =
            {
                float3(0, 0, 0),
                float3(radius, 0, 0)
            };
            var model = Manifold<T>.Revolution(slices, stacks, t => MeshShapeGenerator<T>.EvalBezier(contourn, t), float3(0, 1, 0)).Weld();
            model.ComputeNormals();

            return model.Weld();
        }

        public static Mesh<T> Cylinder(float radius, float zmin, float zmax,int slices = 30, int stacks = 30)
        {
            return Manifold<T>.Surface(slices, stacks, (u, v) =>
            {
                float alpha = u * 2 * pi;

                float x = radius * cos(alpha);
                float y = v * (zmax - zmin) + zmin;
                float z = radius * sin(alpha);

                return float3(x, y, z);
            });
        }

        public static Mesh<T> Cone(float radius, float max_height, float min_height = 0, float top = int.MaxValue, int slices = 30, int stacks = 30)
        {
            return Manifold<T>.Surface(slices, stacks, (u, v) =>
            {
                float max_v = -min_height;
                float min_v = -max_height;

                float alpha = u * 2 * pi;

                float x = v * radius * cos(alpha);
                float y = v * (max_v - min_v) + min_v;
                float z = v * radius * sin(alpha);

                return float3(-x, min(-y, top), -z);
            });
        }
        public static Mesh<T> Hiperboloid(float radius, float max_height, float min_height = 4, int direction = 1, int resX = 30, int resY = 30)
        {
            max_height = max_height == 0 ? 4 : max_height;
            min_height = min_height == 0 ? 4 : min_height;

            return Manifold<T>.Surface(30, 30, (u, v) =>
            {
                float uu = u * 2 * pi;
                float vv = pi / max_height - v * pi / min_height; //Sets upper and lower limmits

                float x = radius * cosh(vv) * cos(uu);
                float z = radius * cosh(vv) * sin(uu);
                float y = sinh(vv);

                return float3(x, direction * y, z);
            });
        }
    }
}