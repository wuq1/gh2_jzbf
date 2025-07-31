using Grasshopper2.Components;
using Grasshopper2.Display;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
using jianjuchanshuhua;
using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace jz
{
    [IoId("2139b6dc-8015-4d16-babd-fcd8d8d7ba5d")]
    public sealed class jzComponent : Component
    {
        public jzComponent() : base(new Nomen(
            "由墙体集合构建room",
            "Description",
            "jianzhu",
            "a"))
        {

        }

        public jzComponent(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddText("", "房间名称", "", Grasshopper2.Parameters.Access.Item);
            inputs.AddGeneric("", "房间内部的墙", "", Grasshopper2.Parameters.Access.Item);
            inputs.AddPoint("", "显示位置", "", Grasshopper2.Parameters.Access.Item,Requirement.MayBeMissing);
            inputs.AddNumber("", "字高", "", Grasshopper2.Parameters.Access.Item, Requirement.MayBeMissing);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("", "ROOM", "",Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The IDataAccess object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        /// 
        Text3d txt = new Text3d("");
        Point3d draw = new Point3d();
        double hight = 0.3;
        string namedraw = "";
        double area = 0;
        protected override void Process(IDataAccess access)
        {
            //
            Point3d center = new Point3d();
            access.GetItem<string>(0, out var name);
            access.GetItem<Walls>(1, out var ws);

            var pts = ws.cypoint;
           
            if (access.GetItem<Point3d>(2,out center))
            {
                access.GetItem<Point3d>(2, out center);
                
            }
            if (access.GetItem<double>(3, out var h))
            {
                access.GetItem<double>(3, out hight);

            }
            if (access.GetItem<Point3d>(2, out center)==false)
            {
                center = centerwitnpts(pts);
            }
            draw = center;
            namedraw = name;
            //




            //
            var room = new Room(name, ws, center);
            var c = pts.Area(Plane.WorldXY);
            area = ((int)c);




            access.SetItem(0, room);
        }
        public override void DisplayWires(DisplayPipeline pipeline, Guises guises, ref BoundingBox extents)
        {
            
            base.DisplayWires(pipeline, guises, ref extents);
            //pipeline.Draw2dText("显示", System.Drawing.Color.Black, new Point2d(100,100), false);
            pipeline.Draw3dText(new Text3d(namedraw/*+"("+area.ToString()+"平方米"+")"*/,new Plane(draw,new Vector3d(0,0,1)),hight), System.Drawing.Color.Black, draw);
        }
        public Point3d centerwitnpts(List<Point3d> ps)
        {
            double x = 0;
            double y = 0;
            double z = 0;
            for (int i = 0; i < ps.Count; i++)
            {
                x = x + ps[i].X;
                y = y + ps[i].Y;
                z = z + ps[i].Z;
            }
            var p = new Point3d(x / ps.Count, y / ps.Count, z / ps.Count);
            return p;

        }
        public List<Point3d> controlpoint(Polyline C)
        {
            List<Point3d> Points = new List<Point3d>();

            for (int i = 0, n = C.ToNurbsCurve().Points.Count; i < n; i++)
            {
                

                Point3d temp = new Point3d(C.ToNurbsCurve().Points[i].X, C.ToNurbsCurve().Points[i].Y, C.ToNurbsCurve().Points[i].Z);

                //Let's add this temp to our point list.
                Points.Add(temp);
            }

            return Points;
        }
        public List<Line> split(Polyline pl)
        {
            List<Line> ls = new List<Line>();
            List<Point3d> pts = controlpoint(pl);
            List<double> ds = new List<double>();
            for (int i = 0; i <pts.Count; i++)
            {


                double tempt;
                pl.ToNurbsCurve().ClosestPoint(pts[i], out tempt);

                ds.Add(tempt);
            }
            var S = pl.ToNurbsCurve().Split(ds);
            for (int j = 0; j < S.Length; j++)
            {
                ls.Add(new Line(S[j].PointAtStart, S[j].PointAtEnd));
            }
            return ls;
        }
        public List<Line> splits(List<Polyline> pls)
        {
            List<Line> ls = new List<Line>();
            for (int i = 0; i < pls.Count; i++)
            {
                var pnls = split(pls[i]);
                for (int j = 0; j < pnls.Count; j++)
                {
                    ls.Add(pnls[j]);
                }
            }
            return ls;
        }
    }
}
