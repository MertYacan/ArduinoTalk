using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ArduinoTalk
{
    public class PathMaker : GH_Component
    {
        public static List<Point3d> inputPoints = new List<Point3d>();
        public static List<Point3d> analysisPoint3d = new List<Point3d>();
        Point3d[] editedPoints;
        //rotationDegrees is in /180 degree mode cuz we will ask for 180 rotation step number manually
        List<Double> rotationDegrees = new List<Double>();
        List<int> rotationAction = new List<int>();
        List<GeometryBase> calculationGeometry = new List<GeometryBase>();
        public static int actionCount = 0;
        public static Boolean calculated = false;
        public static Boolean next = false;

        //to give rotations a actionNumber we will need this
        int rotationFound = 0;
        int editedPointCount = 0;
        
        //these are for sending info
        int rightRot = 0;
        int leftRot = 0;
        Boolean analysis = false;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public PathMaker()
          : base("Path Maker", "Path Maker",
              "Create paths for robot",
              "Mert's Tools", "Robot")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Calculate", "Calculate", "Calculation is needed to start", GH_ParamAccess.item, false);
            pManager.AddPointParameter("Path Points", "Points", "Points to go", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Analysis Points", "Analysis Points", "Points where will be analysis", GH_ParamAccess.list);
            pManager.AddNumberParameter("CenterToWheel", "CenterToWheel", "Robot's center to wheel distance", GH_ParamAccess.item, 5.95);
            pManager.AddIntegerParameter("180Rot", "180Rot", "Steps for 180 rotation", GH_ParamAccess.item, 1240);
            pManager.AddIntegerParameter("10CmRot", "10CmRot", "Steps for 1 cm distance", GH_ParamAccess.item, 335);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("LeftRot", "LeftRot", "How many times left step will rotate", GH_ParamAccess.item);
            pManager.AddNumberParameter("RightRot", "RightRot", "How many times left step will rotate", GH_ParamAccess.item);
            pManager.AddBooleanParameter("IsAnalysisPoint", "isAnalysisPoint", "Should bot get analysis?", GH_ParamAccess.item);
            pManager.AddGeometryParameter("CalculationGeometry", "CalculationGeometry", "Geometries used for calculation", GH_ParamAccess.list);
            pManager.AddPointParameter("TestPoints", "TestPoints", "Jftest", GH_ParamAccess.list);
            pManager.AddIntegerParameter("TestAct", "TestAct", "TestAct", GH_ParamAccess.item);
            pManager.AddIntegerParameter("TestAct2", "TestAct2", "TestAct", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Boolean calc = false;
            if (!DA.GetData(0, ref calc)) return;
            List<int> analysisPoints = new List<int>();
            if (!DA.GetDataList(2, analysisPoints)) return;

            if (calc) {
                //just resetting anything if the calculation is called for multiple times
                rotationAction.Clear();
                rotationDegrees.Clear();
                next = false;
                actionCount = 0;
                calculationGeometry.Clear();
                inputPoints.Clear();
                Rhino.DocObjects.RhinoObject[] rhobjs = Rhino.RhinoDoc.ActiveDoc.Objects.FindByLayer("Analysis");
                if (rhobjs != null)
                {
                    for (int j = 0; j < rhobjs.Length; j++)
                    {
                        Rhino.RhinoDoc.ActiveDoc.Objects.Delete(rhobjs[j], true);
                    }
                }

                //we calculate the route. if there is a turn we need to adjust the points 
                //if not we just add 0 1 1 2 2 3 3 4th points to the array
                if (!DA.GetDataList(1, inputPoints)) return;
                Double radius = 1;
                if (!DA.GetData(3, ref radius)) return;
                //2 times - 1 bigger array because we put all points twice except first and last ones
                //could make it - 2 but then: if we input only one point then we would get error
                if (inputPoints.Count > 1)
                {
                    editedPoints = new Point3d[inputPoints.Count * 2 - 2];
                }
                else {
                    editedPoints = new Point3d[inputPoints.Count * 2 - 1];
                }
                for (int i = 0; i < inputPoints.Count - 1; i++) {
                    if (i == 0) { 
                        editedPoints[editedPointCount++] = inputPoints[i];
                        continue;
                    }
                    Line lTemp1 = new Line(inputPoints[i - 1], inputPoints[i]);
                    Line lTemp2 = new Line(inputPoints[i], inputPoints[i + 1]);
                    Vector3d vTemp1 = (lTemp1.Direction);
                    vTemp1.Unitize();
                    Vector3d vTemp2 = (lTemp2.Direction);
                    vTemp2.Unitize();
                    //if lines are parallel then we just accept points without creating a rotation event or changing them
                    if (vTemp1.EpsilonEquals(vTemp2, 0.1))
                    {
                        editedPoints[editedPointCount++] = inputPoints[i];
                        Circle circle = new Circle(inputPoints[i], radius*0.5); // highlighting the analysis point 
                        analysisPoint3d.Add(inputPoints[i]);
                        calculationGeometry.Add(circle.ToNurbsCurve());
                        // analysis point text
                        Rhino.RhinoDoc.ActiveDoc.Layers.SetCurrentLayerIndex(1, true);
                        Rhino.RhinoDoc.ActiveDoc.Objects.AddText(new Rhino.Display.Text3d("Analysis Point",new Plane(inputPoints[i],Rhino.Geometry.Vector3d.ZAxis),1));
                    }
                    //if lines aren't parallel, we will need to find the point where one of the wheels will rest while other wheel will rotate and complete the rotation task
                    //to do that we will need to know which direction our robot will turn. if rotating left, the left wheel will stay still, and vice versa when its a right turn
                    //then we will need to find where to stop the robot so that we will be able to get into the next path line
                    else
                    {
                        String textToWrite = "";                        
                        //divided by PI and not multiplied by 180 because we will get 180 rot value as input and use them together
                        double degree = Math.Acos((vTemp1.X * vTemp2.X) + (vTemp1.Y * vTemp2.Y)) / Math.PI;
                        double degreeForCalc = (180 - (degree*180.0)) / 2.0;
                        double circleCenterDist = radius / Math.Sin((degreeForCalc/180)*Math.PI);
                        if (circleCenterDist < 0) circleCenterDist *= -1;
                        //cross product  if - right turn, else left turn
                        if (((vTemp1.X * vTemp2.Y) - (vTemp1.Y * vTemp2.X)) < 0)
                        {
                            textToWrite += "Rotation to Right\n Degree = " + ((int)(degree*180)).ToString();
                            degree *= -1;
                        }
                        else {
                            textToWrite += "Rotation to Left\n Degree = " + ((int)(degree*180)).ToString();
                        }
                        rotationDegrees.Add(degree);
                        
                        Vector3d midVec = vTemp2 - vTemp1;
                        midVec.Unitize();
                        midVec *= circleCenterDist;
                        Point3d midVecPoint = new Point3d(inputPoints[i]);
                        midVecPoint += midVec;
                        Circle circle = new Circle(midVecPoint, radius);
                        Point3d pointOnLine1 = lTemp1.ClosestPoint(midVecPoint, false);
                        Point3d pointOnLine2 = lTemp2.ClosestPoint(midVecPoint, false);
                        
                        editedPoints[editedPointCount++] = pointOnLine1;
                        editedPoints[editedPointCount++] = pointOnLine2;
                        rotationAction.Add(i + rotationFound);
                        rotationFound++;

                        calculationGeometry.Add(circle.ToNurbsCurve());
                        calculationGeometry.Add(new LineCurve(midVecPoint, pointOnLine1));
                        calculationGeometry.Add(new LineCurve(midVecPoint, pointOnLine2));
                        Rhino.RhinoDoc.ActiveDoc.Layers.SetCurrentLayerIndex(1, true);
                        Rhino.RhinoDoc.ActiveDoc.Objects.AddText(new Rhino.Display.Text3d(textToWrite, new Plane(inputPoints[i],Rhino.Geometry.Vector3d.ZAxis), 1));
                    }
                    if (i == inputPoints.Count - 2) {
                        editedPoints[editedPointCount++] = inputPoints[i+1];
                    }
                }

                for (int i = 0; i < editedPoints.Length-1; i++) {
                    calculationGeometry.Add(new LineCurve(editedPoints[i],editedPoints[i+1]));
                }

                calculated = true;
                DA.SetDataList("TestPoints", editedPoints);
            }
            //if calculation complete and next results are needed
            if(calculated && next){
                next = false;
                //if gonna turn
                if (rotationAction.Count>0 && rotationAction[0] == actionCount) {
                    //if minus then its left turn
                    int rotFor180 = 1;
                    if (!DA.GetData(4, ref rotFor180)) return;

                    if (rotationDegrees[0]<0) {
                        rightRot = (int)(rotationDegrees[0] * rotFor180);
                        leftRot = 0;
                    }
                    //otherwise right turn
                    else {
                        leftRot = (int)(rotationDegrees[0] * rotFor180);
                        rightRot = 0;
                    }
                    rotationAction.RemoveAt(0);
                    rotationDegrees.RemoveAt(0);
                    //there wont be any analysis at rotation points according to my design
                    analysis = false;
                }
                //if gonna go straight
                else {
                    int rotFor10cm = 335;
                    if (!DA.GetData(5, ref rotFor10cm)) return;
                    //this will give distance to go in centimeters
                    double distanceToGo = editedPoints[actionCount].DistanceTo(editedPoints[actionCount + 1]);
                    rightRot = leftRot = (int)((distanceToGo / 10)*rotFor10cm);
                    //check if we need to get analysis
                    if (analysisPoints.Contains(actionCount)){
                        analysis = true;
                    }
                    else {
                        analysis = false;
                    }
                }
                ArduinoTalkComponent.start = true;
            }

            //here are other stuff
            DA.SetData("LeftRot", leftRot);
            DA.SetData("RightRot", rightRot);
            DA.SetData("IsAnalysisPoint", analysis);
            DA.SetData("TestAct", actionCount);
            DA.SetDataList("CalculationGeometry", calculationGeometry);
            DA.SetDataList("TestPoints", editedPoints);
            DA.SetDataList("TestAct2", rotationAction);
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
                return ArduinoTalk.Properties.Resources.path.ToBitmap();
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("17a8aad5-3fc2-4002-bc57-038fe6223f57"); }
        }
    }
}