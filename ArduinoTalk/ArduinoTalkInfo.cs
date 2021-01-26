using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ArduinoTalk
{
    public class ArduinoTalkInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ArduinoTalk";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Crazy stuff";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("1213ae6e-f7e7-4130-b829-c6a60fcb0c2e");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Mert Yacan";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "mertyacan.com";
            }
        }
    }
}
