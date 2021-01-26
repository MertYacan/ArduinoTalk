using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Globalization;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace ArduinoTalk
{
    public class ArduinoTalkComponent : GH_Component
    {
        //public cause counter component will access this
        public static Boolean start = false;
        public static List<Double> results = new List<Double>();
        String outS = "";
        String deneme = "";
        int panelNo = 0;

        public ArduinoTalkComponent()
          : base("Talk", "Talk",
              "Send and receive strings between pc and arduino",
              "Mert's Tools", "Robot")
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("LeftRot", "LR", "How many times left motor steps?", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("RightRot", "RR", "How many times right motor steps?", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Analysis", "Analysis", "Do we need analysis?", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Results", "Results", "Results are here", GH_ParamAccess.list);
            pManager.AddTextParameter("Out", "Out", "Results are here", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (start == true) {
                start = false;

                String inMessage = "";

                int lCount = 0;
                int rCount = 0;
                Boolean analysis = false;
                if (!DA.GetData(0, ref lCount)) return;
                if (!DA.GetData(1, ref rCount)) return;
                if (!DA.GetData(2, ref analysis)) return;

                //lets prepare the message for arduino
                //we will seperate the message at x's so we will get lRot x rRot x Analysis
                String message = "";
                message += rCount.ToString() + "x" + lCount.ToString() + "x";
                if (analysis)
                {
                    message += "1x";
                }
                else {
                    message += "0x";
                }

                try {
                    Connect.serial.Write(message);
                    inMessage = Connect.serial.ReadLine();
                    outS = message;
                }catch {
                    return;
                }

                //DA.SetData("Out", outS);

                //while(inMessage == null || inMessage.Equals("")) inMessage = Connect.serial.ReadLine();

                if (inMessage.Equals("ok") || inMessage == "ok") {
                    DA.SetData("Out", inMessage);
                    PathMaker.actionCount++;
                    Counter.count = true;
                    
                }
                else {
                    deneme = inMessage;
                    DA.SetData("Out", deneme);
                    PathMaker.actionCount++;
                    Counter.count = true;
                    /*
                    PathMaker.actionCount++;
                    results.Add(20.0);
                    //results.Add(Double.Parse(inMessage));
                    DA.SetDataList("Results", results);
                    Solver.AdjustPanel(panelNo);
                    panelNo++;
                    */
                }
                
            } else {
                DA.SetDataList("Results", results);
                //DA.SetData("Out", outS);
                DA.SetData("Out", deneme);
            }

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return ArduinoTalk.Properties.Resources.talk.ToBitmap();
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("d141e3e0-1a88-49d8-8263-d013be4cbb64"); }
        }
    }
}
