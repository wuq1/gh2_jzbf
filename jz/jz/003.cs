using Grasshopper2.Components;
using Grasshopper2.Display;
using Grasshopper2.UI;
using GrasshopperIO;
using jianjuchanshuhua;
using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Linq;

namespace jz003
{
    [IoId("A0260D45-3A69-48F0-A9AC-656D2AF56E79")]
    public sealed class jzComponent : Component
    {
        public jzComponent() : base(new Nomen(
            "合并墙体",
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
            inputs.AddGeneric("", "墙体", "", Grasshopper2.Parameters.Access.Twig);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("", "墙体集合", "");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The IDataAccess object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
        {
            access.GetTwig<Wall>(0, out var wally);
            var walls = wally.ToItemArray(Grasshopper2.Data.ToArrayMethod.Always).ToList<Wall>();
            Walls ws = new Walls(walls);
           /* double h = ws.walls[0].hight;
            for (int i = 0; i < ws.walls.Count; i++)
            {
                ws.walls[i].hight = h;
            }
           */
            access.SetItem(0, ws);
        }
        public override void DisplayWires(DisplayPipeline pipeline, Guises guises, ref BoundingBox extents)
        {

            base.DisplayWires(pipeline, guises, ref extents);

        }
    }
}