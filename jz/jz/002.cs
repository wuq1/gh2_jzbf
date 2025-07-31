using Grasshopper2.Components;
using Grasshopper2.Display;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
using jianjuchanshuhua;
using Rhino.Display;
using Rhino.Geometry;
using System;

namespace jz002
{
    [IoId("E67BCF7E-1326-406B-A71C-A7E36DFB6CA8")]
    public sealed class jzComponent : Component
    {
        public jzComponent() : base(new Nomen(
            "构成墙体",
            "Description",
            "jianzhu",
            "Section"))
        {

        }

        public jzComponent(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddCurve("","墙基线", "", Grasshopper2.Parameters.Access.Item);
            inputs.AddNumber("", "墙厚", "", Grasshopper2.Parameters.Access.Item,Requirement.MayBeNull).Set(0.2);
            inputs.AddNumber("", "墙高", "", Grasshopper2.Parameters.Access.Item, Requirement.MayBeNull).Set(3);
            inputs.AddNumber("", "到地面高度", "", Grasshopper2.Parameters.Access.Item, Requirement.MayBeNull).Set(0.8);
            inputs.AddNumber("", "窗户高度", "", Grasshopper2.Parameters.Access.Item, Requirement.MayBeNull).Set(1.2);
            inputs.AddNumber("", "窗户起点", "", Grasshopper2.Parameters.Access.Item, Requirement.MayBeNull).Set(0.2);
            inputs.AddNumber("", "窗户终点", "", Grasshopper2.Parameters.Access.Item, Requirement.MayBeNull).Set(0.8);




        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("", "墙体", "");
           // outputs.AddGeneric("", "窗户", "");

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The IDataAccess object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
        {
            access.GetItem<Curve>(0, out var c);
            access.GetItem<double>(1, out var d);
            access.GetItem<double>(2,out var h);
            access.GetItem<double>(3, out var hd);
            access.GetItem<double>(4, out var hw);
            access.GetItem<double>(5, out var start);
            access.GetItem<double>(6, out var end);






            //
            Wall w = new Wall(c.PointAtStart, c.PointAtEnd, h, d);
            Window win = new Window(w, start, end, hd, hw);
            w.addwindow(win);
            access.SetItem(0, w);
           // access.SetItem(1, win.WindowView());

        }
        public override void DisplayWires(DisplayPipeline pipeline, Guises guises, ref BoundingBox extents)
        {

            base.DisplayWires(pipeline, guises, ref extents);

        }
    }
}