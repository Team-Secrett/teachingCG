using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using GMath;
using Renderer;
using Rendering;
using static GMath.Gfx;
using static Renderer.Program;

namespace Renderer
{
    public class Model<V> where V : struct, IVertex<V>, INormalVertex<V>, ICoordinatesVertex<V>
    {
        public Model(Mesh<V> obj, Material m)
        {
            this.obj = obj;
            this.m = m;
        }

        public Material m { get; set; }

        public Mesh<V> obj { get; set; }

        public IRaycastGeometry<V> AsRayCast(RaycastingMeshMode mode = RaycastingMeshMode.Grid)
        {
            return obj.AsRaycast(mode);
        }

        public void AddToScene(Scene<V, Material> scene)
        {
            scene.Add(AsRayCast(), m, Transforms.Identity);
        }
    }
}
