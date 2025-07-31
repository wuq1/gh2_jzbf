using Grasshopper2.Components;
using Grasshopper2.Display;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
using jianjuchanshuhua;
using Rhino.Display;
using Rhino.Geometry;
using System;

namespace jz002no
{
    [IoId("CA2AE9FA-F640-4A38-835A-30572DEEB87B")]
    public sealed class jzComponent : Component
    {
        public jzComponent() : base(new Nomen(
            "构成无窗墙体",
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


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("", "墙体", "");
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




            //
            Wall w = new Wall(c.PointAtStart, c.PointAtEnd, h, d);

            access.SetItem(0, w);
        }
        public override void DisplayWires(DisplayPipeline pipeline, Guises guises, ref BoundingBox extents)
        {

            base.DisplayWires(pipeline, guises, ref extents);

        }
    }
}