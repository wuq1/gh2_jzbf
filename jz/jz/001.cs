using Grasshopper2.Components;
using Grasshopper2.Display;
using Grasshopper2.UI;
using GrasshopperIO;
using jianjuchanshuhua;
using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace jz001
{
    [IoId("B7EAE4FD-27A2-4790-8B7E-20309B9AB3B1")]
    public sealed class jzComponent : Component
    {
        public jzComponent() : base(new Nomen(
            "roomview",
            "Description",
            "jianzhu",
            "b"))
        {

        }

        public jzComponent(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("", "ROOM", "", Grasshopper2.Parameters.Access.Item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("", "无窗墙体", "", Grasshopper2.Parameters.Access.Twig);
            outputs.AddGeneric("", "有窗墙体", "", Grasshopper2.Parameters.Access.Twig);

            outputs.AddGeneric("", "墙角", "", Grasshopper2.Parameters.Access.Twig);
            outputs.AddGeneric("", "窗户", "", Grasshopper2.Parameters.Access.Twig);


        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The IDataAccess object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
        {
            access.GetItem<Room>(0, out var room);
            List<object> view1 = new List<object>();
            List<object> view2 = new List<object>();
            List<object> view3 = new List<object>();
            List<object> view4 = new List<object>();



            for (int i = 0; i < room.ws.angles.Count; i++)
            {
                view3.Add(room.ws.angles[i].view);
               
            }

            for (int i = 0; i < room.ws.walls.Count; i++)
            {
                
                if(room.ws.walls[i].havewin)
                {
                    view4.Add(room.ws.walls[i].win.WindowView());
                    view2.Add(room.ws.walls[i].WallView());

                }
                if (room.ws.walls[i].havewin==false)
                {
                    view1.Add(room.ws.walls[i].WallView());
                }



            }

            //
            access.SetTwig(0, view1.ToArray());
            access.SetTwig(1, view2.ToArray());
            access.SetTwig(2, view3.ToArray());
            access.SetTwig(3, view4.ToArray());



        }
        public override void DisplayWires(DisplayPipeline pipeline, Guises guises, ref BoundingBox extents)
        {

            base.DisplayWires(pipeline, guises, ref extents);

        }
    }
}