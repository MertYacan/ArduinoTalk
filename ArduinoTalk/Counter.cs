using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ArduinoTalk
{
    public class Counter : GH_Component
    {
        System.Windows.Forms.Timer timer = null;
        int num = 10;
        public static Boolean count = false; //public because other components will access
        Boolean counting = false;
        Boolean paused = false;
        Boolean timerOn = false;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Counter()
          : base("Counter", "Counter",
              "Counts before sending next data to arduino.",
              "Mert's Tools", "Robot")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Wait Time", "Wait Time", "Time to wait in seconds", GH_ParamAccess.item, 5);
            pManager.AddBooleanParameter("Pause", "Pause", "True when should be stopped", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Start", "Start", "True when should be started", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Out", "Out", "Status info", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Boolean start = false;
            if (!DA.GetData(2, ref start)) return;

            Boolean tempPause = false;
            if (!DA.GetData(1, ref tempPause)) return;
            paused = tempPause;

            if (start) {
                //after this started, next loops will be started by other components count is public so they will able to access
                count = true;
            }

            //if should count but not counting
            if (count && !counting) {
                int tempNum = 1;
                if (!DA.GetData(0, ref tempNum)) return;
                num = tempNum;

                if (!timerOn) {
                    //set timer
                    timer = new System.Windows.Forms.Timer();
                    timer.Interval = 1000;
                    timer.Tick += new EventHandler(Ticker);
                    timer.Enabled = true;
                    timerOn = true;
                }                

                counting = true;
                count = false;
            }

            //string out
            if (!counting) {
                String temp = "Waiting...";
                DA.SetData("Out", temp);
            }
            else {
                String temp = "Counting = ";
                temp += num.ToString();
                DA.SetData("Out", temp);
            }

            //when its ready to send command
            if (num == 0 && counting) {
                timer = null;
                counting = false;

                if (PathMaker.calculated) PathMaker.next = true;
                //call arduino talk and path maker for next step
            }
        }

        protected void Ticker(object sender, EventArgs e) {
            if (num > 0 && !paused)
            {
                num--;
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return ArduinoTalk.Properties.Resources.counter.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e7e4eee2-6450-4fd6-8868-a9f3fd2f8703"); }
        }
    }
}