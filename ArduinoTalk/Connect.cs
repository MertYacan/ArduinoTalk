using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using System.Text;
using System.IO.Ports;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace ArduinoTalk
{
    public class Connect : GH_Component
    {

        public static SerialPort serial = new SerialPort();


        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Connect()
          : base("Connect", "Connect",
              "Just does some crazy stuff",
              "Mert's Tools", "Robot")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Port", "P", "Port Num", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("Baud", "B", "Baud Rate", GH_ParamAccess.item, 115200);
            pManager.AddBooleanParameter("Open", "O", "Open Port", GH_ParamAccess.item, false);
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
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int port = 10;
            int baud = 115200;
            bool open = false;
            string message = "Nothing Yet";

            if (!DA.GetData(0, ref port)) { return; }
            if (!DA.GetData(1, ref baud)) { return; }
            if (!DA.GetData(2, ref open)) { return; }



            //sets the number of milliseconds before a time-out occurs when a write or read operation does not finish.


            if (open == true && !serial.IsOpen)
            {
                try
                {
                    serial.PortName = "COM" + port.ToString();
                    serial.BaudRate = baud;
                    serial.WriteTimeout = 30000;
                    serial.ReadTimeout = 30000;
                    serial.Open();

                    message = "Port Open";
                }
                catch
                {
                    message = "Error opening Serial Port";
                }
            }

            if (open == false && serial.IsOpen)
            {
                serial.DiscardInBuffer();
                serial.DiscardOutBuffer();
                serial.Dispose();
                serial.Close();
                message = "Port Closed";
            }

            DA.SetData(0, message);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return ArduinoTalk.Properties.Resources.connect.ToBitmap();
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("bd4a5e27-5b48-49f6-bf37-ef5683b5f788"); }
        }
    }
}
