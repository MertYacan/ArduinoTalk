using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace ArduinoTalk
{
    public class Solver : GH_Component
    {
        public static Interval temp;
        public static Interval degree;
        public static List<int> panelDegrees = new List<int>();
        public static int steps;
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Solver()
          : base("Solver", "Solver",
              "Solves the analysis result according to the input",
              "Mert's Tools", "Robot")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Temperature Interval", "Temperature", "The temperature interval to calculate", GH_ParamAccess.item);
            pManager.AddIntervalParameter("Panel Degree Interval", "Panel", "The panel degree interval to calculate", GH_ParamAccess.item);
            pManager.AddNumberParameter("Gear Ratio", "Gear", "The gear ratio", GH_ParamAccess.item, 9);
            pManager.AddIntegerParameter("Panel Degrees", "Panel Degrees", "The panel degrees at the beginning", GH_ParamAccess.list);
            pManager.AddIntegerParameter("180 to Steps", "180 to Steps", "How many steps to rotate 180", GH_ParamAccess.item, 100);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Out", "Out", "Message", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(0, ref temp)) return;
            if (!DA.GetData(1, ref degree)) return;
            if (panelDegrees.Count<1) {
                if (!DA.GetDataList(3, panelDegrees)) return;
            }
            if (!DA.GetData(4, ref steps)) return;

            String mes = "";
            mes += temp.ToString();
            mes += degree.ToString();

            DA.SetData("Out", mes);
        }

        public static void AdjustPanel(int panelNo) {
            
                int tempRes = (int)ArduinoTalkComponent.results[panelNo];
                if (tempRes < temp.Min) {
                    tempRes = (int)temp.Min;
                } else if (tempRes > temp.Max) {
                    tempRes = (int)temp.Max;
                }
                int tempDegree = Map(tempRes, (int)temp.Min, (int)temp.Max, (int)degree.Min, (int)degree.Max);
                int differenceDegree = tempDegree - panelDegrees[panelNo];
                int rotToRotate = 0;
                String inMessage = null;

                if (differenceDegree > 10) {
                    int tempRot = 0;
                    int tempTest = differenceDegree;
                    while (tempTest > 10) {
                        tempTest -= 20;
                        tempRot++;
                    }
                    rotToRotate = tempRot;
                    // aaax0x2x
                    String message = "";
                    message += rotToRotate*steps + "x0x2x";
                    Connect.serial.Write(message);
                    inMessage = Connect.serial.ReadLine();
                    panelDegrees[panelNo] += rotToRotate * 20;
                } else if (differenceDegree < -10) {
                    int tempRot = 0;
                    int tempTest = differenceDegree;
                    while (tempTest < -10) {
                        tempTest += 20;
                        tempRot++;
                    }
                    rotToRotate = tempRot;
                    // 0xaaax2x
                    String message = "0x";
                    message += rotToRotate * steps + "x2x";
                    Connect.serial.Write(message);
                    inMessage = Connect.serial.ReadLine();
                    panelDegrees[panelNo] -= rotToRotate * 20;
                }
            Counter.count = true;
            
        }

        private static int Map(int value, int fromLow, int fromHigh, int toLow, int toHigh)
        {
            return (int)((value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow);
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
                return ArduinoTalk.Properties.Resources.solver.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8ecbddb0-6914-4537-8123-1173d4d82852"); }
        }
    }
}