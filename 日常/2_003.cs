using Grasshopper2.Components;
using Grasshopper2.Display;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;

using Rhino.Display;
using Rhino.Geometry;
using Rhino.Render;
using Rhino.Render.ChangeQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using time;
using Mesh = Rhino.Geometry.Mesh;

namespace jz203
{
    [IoId("33B79485-012D-40F1-B9D2-878EC904DF51")]
    public sealed class jzComponent : Component
    {
        public jzComponent() : base(new Nomen(
            "创建一个材质",
            "Description",
            "qta",
            "Section"))
        {

        }

        public jzComponent(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            // inputs.AddGeneric("", "点数据", "", Grasshopper2.Parameters.Access.Item);
            // inputs.AddGeneric("", "个数", "", Grasshopper2.Parameters.Access.Item,Requirement.MayBeMissing).Set(10);
            inputs.AddGeneric("File", "Brep", "The file to read", Grasshopper2.Parameters.Access.Item);
            inputs.AddText("File", "颜色纹理", "The file to read", Grasshopper2.Parameters.Access.Item, Requirement.MayBeMissing);
            inputs.AddText("File", "凹凸纹理", "The file to read", Grasshopper2.Parameters.Access.Item, Requirement.MayBeMissing);
            inputs.AddText("File", "环境纹理", "The file to read", Grasshopper2.Parameters.Access.Item, Requirement.MayBeMissing);
            inputs.AddText("File", "透明纹理", "The file to read", Grasshopper2.Parameters.Access.Item, Requirement.MayBeMissing);

            inputs.AddBox("File", "Box", "The file to read", Grasshopper2.Parameters.Access.Item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {

            outputs.AddGeneric("Output", "View", "Output description", Grasshopper2.Parameters.Access.Twig);


        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The IDataAccess object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        /// 
        List<Mesh> mview = new List<Mesh>();
        Brep b = new Brep();
        Rhino.DocObjects.Material mat = new Rhino.DocObjects.Material();
        Rhino.Display.DisplayMaterial mat2 = new Rhino.Display.DisplayMaterial();

        protected override void Process(IDataAccess access)
        {
            mview = new List<Mesh>();
            b = new Brep();
            mat = new Rhino.DocObjects.Material();
            access.GetItem<Box>(5, out var box);
            if (access.GetItem<string>(1, out var bit))
            {
                access.GetItem<string>(1, out var bit0);
                mat.SetBitmapTexture(bit0);
              //  mat2.SetBitmapTexture( bit0,true);
            }
            if (access.GetItem<string>(2, out var bit2))
            {
                access.GetItem<string>(2, out var bit02);
                mat.SetBumpTexture(bit02);

            }
            if (access.GetItem<string>(3, out var bit3))
            {
                access.GetItem<string>(3, out var bit03);
                mat.SetEnvironmentTexture(bit03);

            }

            if(access.GetItem<string>(4, out var bit4))
            {
                access.GetItem<string>(4, out var bit04);
             
                mat.SetTransparencyTexture(bit04);

            }

            if (access.GetItem<Brep>(0,out var b0))
            {
                access.GetItem<Brep>(0, out var b1);
                b = b1;
            }
            if (access.GetItem<Box>(0, out var b2))
            {
                access.GetItem<Box>(0, out var b21);
                
                mview = (Mesh.CreateFromBrep(b21.ToBrep()).ToList<Mesh>());
            }
            if(access.GetItem<Brep>(0,out var b3))
            {
                access.GetItem<Brep>(0, out var b31);
                mview=(Mesh.CreateFromBrep(b31).ToList<Mesh>());
            }
            if (access.GetItem<Surface>(0, out var b4))
            {
                access.GetItem<Surface>(0, out var b41);
                mview = (Mesh.CreateFromBrep(b41.ToBrep()).ToList<Mesh>());
            }
            if(access.GetItem<Mesh>(0, out var b66))
            {
                access.GetItem<Mesh>(0, out var b67);
                mview.Add( b67);
            }
            if (access.GetItem<Sphere>(0, out var b5))
            {
                access.GetItem<Sphere>(0, out var b51);
                mview = (Mesh.CreateFromBrep(b51.ToBrep()).ToList<Mesh>());
            }
            //
            var textu = TextureMapping.CreateBoxMapping(box.Plane, box.X, box.Y, box.Z, true);
            if (access.GetItem<Sphere>(0, out var b52))
            {
                access.GetItem<Sphere>(0, out var b512);
                 textu = TextureMapping.CreateSphereMapping(b512);
            }
            else
            {
                textu = TextureMapping.CreateBoxMapping(box.Plane, box.X, box.Y, box.Z, true);
            }

                for (int i = 0; i < mview.Count; i++)
                {
                    mview[i].TextureCoordinates.SetTextureCoordinates(textu);
                }
            //access.SetTwig(0, mview.ToArray());
            //
        }
        public override void DisplayWires(DisplayPipeline pipeline, Guises guises, ref BoundingBox extents)
        {

            base.DisplayWires(pipeline, guises, ref extents);
            // pipeline.DrawBrepShaded(b, new DisplayMaterial(mat));
            for (int i = 0; i < mview.Count; i++)
            {
                pipeline.DrawMeshShaded(mview[i], new DisplayMaterial(mat));
            }
           

        }
    }
}