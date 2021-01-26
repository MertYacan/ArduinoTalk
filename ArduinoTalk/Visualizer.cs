using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ArduinoTalk
{
    public class Visualizer : GH_Component
    {
        public List<Double> analysisResults = new List<Double>();
        public List<Brep> panelGeo = new List<Brep>();
        public List<Brep> baseGeo = new List<Brep>();
        Brep robo = null;
        int roboLoc = 0;
        List<System.Drawing.Color> colors = new List<System.Drawing.Color>();
        public Boolean doAnalysis = false;
        public Boolean ready = false;
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Visualizer()
          : base("Visualizer", "Visualizer",
              "Simulates all the process.",
              "Mert's Tools", "Robot")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Panel Base", "Panel Base", "The base geometry of the panel", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Panel", "Panel", "The adjustable geometry of the panel", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Robot", "Robot", "The robot geometry", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Start", "Start", "To start the visualization", GH_ParamAccess.item, false);
            pManager.AddVectorParameter("Distance Vector", "Distance Vector", "The distance between point and panel base", GH_ParamAccess.item, Rhino.Geometry.Vector3d.XAxis);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddGeometryParameter("Panel Geo", "Panel Geo", "All panel geometries", GH_ParamAccess.list);
            //pManager.AddGeometryParameter("Robot", "Robot", "The robot geometry at current location", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //if first time put the panel geo to memory only once
            Boolean start = false;
            if (!DA.GetData(3, ref start)) return;

            if (!ready && start && PathMaker.analysisPoint3d.Count>0) {
                Vector3d moveTemp = new Vector3d(0,0,0);
                if (!DA.GetData(4, ref moveTemp)) return;
                List<Brep> panelTemp = new List<Brep>();
                Brep baseTemp = null;
                if (!DA.GetData(1, ref panelTemp)) return;
                if (!DA.GetData(0, ref baseTemp)) return;
                for (int i = 0; i < PathMaker.analysisPoint3d.Count; i++) {
                    List<Brep> panelTemp2 = new List<Brep>(panelTemp);
                    Brep baseTemp2 = baseTemp.DuplicateBrep();
                    Vector3d totVec = new Vector3d(0, 0, 0);
                    totVec += new Rhino.Geometry.Vector3d(PathMaker.analysisPoint3d[i]);
                    totVec += moveTemp;
                    baseTemp.Translate(totVec);
                    baseGeo.Add(baseTemp);

                    for(int j = 0; j < panelTemp2.Count; j++) {
                        Brep panelTempBrep = panelTemp2[i];
                        panelTempBrep.Translate(totVec);
                        panelGeo.Add(panelTempBrep);
                    }
                }
                if (!DA.GetData(2, ref robo)) return;

                ready = true;
            }

            foreach (Double d in analysisResults) {
                colors.Clear();
                if (d < 15)
                {
                    colors.Add(System.Drawing.Color.FromArgb(0, 255, 0));
                }
                else {
                    int r = 0 + Map((int)d - 15, 0, 85, 0, 255);
                    int g = 255 - Map((int)d - 15, 0, 85, 0, 255);
                    colors.Add(System.Drawing.Color.FromArgb(r, g, 0));
                }
            }

            if (PathMaker.actionCount>roboLoc) {
                Vector3d vecMove = new Vector3d(0, 0, 0);
                Vector3d vecTemp1 = new Vector3d(PathMaker.inputPoints[roboLoc]);
                Vector3d vecTemp2 = new Vector3d(PathMaker.inputPoints[PathMaker.actionCount]);
                vecMove += vecTemp2 - vecTemp1;

                robo.Translate(vecMove);
                roboLoc = PathMaker.actionCount;
            }
        }
        private static int Map(int value, int fromLow, int fromHigh, int toLow, int toHigh)
        {
            return (int)((value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow);
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (ready) {
                //System.Drawing.Color clr = System.Drawing.Color.FromArgb(120, 120, 120);
                //Rhino.Display.DisplayMaterial mat = new Rhino.Display.DisplayMaterial(clr);
                for (int i = 0; i < panelGeo.Count; i++) {
                    Rhino.Display.DisplayMaterial mat = new Rhino.Display.DisplayMaterial(colors[i]);
                    args.Display.DrawBrepShaded(panelGeo[i], mat);
                    args.Display.DrawBrepWires(panelGeo[i], System.Drawing.Color.Black);
                    mat.Dispose();
                }
                args.Display.DrawBrepWires(robo, System.Drawing.Color.Black);
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
                return ArduinoTalk.Properties.Resources.simulator.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e498cd75-ba03-4e6a-a1ce-693e19d5e856"); }
        }
    }
}