using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using ScreenManagement;

namespace PowershellScreenManagement
{
    // Declare the class as a cmdlet and specify and 
    // appropriate verb and noun for the cmdlet name.
    [Cmdlet(VerbsCommon.Get, "ScreenDevices")]
    public class GetScreenDevicesCommand : Cmdlet
    {
        /*// Declare the parameters for the cmdlet.
        [Parameter(Mandatory = true)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string name;*/

        // Overide the ProcessRecord method to process
        // the supplied user name and write out a 
        // greeting to the user by calling the WriteObject
        // method.
        protected override void ProcessRecord()
        {
            foreach(DISPLAY_DEVICE d in ScreenDevice.GetDesktopDevices())
            {
                WriteObject(d);
            }
        }
    }
}
