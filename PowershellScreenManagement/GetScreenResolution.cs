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
    [Cmdlet(VerbsCommon.Get, "ScreenResolution")]
    public class GetScreenResolution : Cmdlet
    {
        // Declare the parameters for the cmdlet.
        [Parameter(Mandatory = true, Position = 1)]
        public string DeviceName;

        // Declare the parameters for the cmdlet.
        [Parameter(Mandatory = false, Position = 2)]
        [ValidateSet("All", "Current", "Registry")]
        public string QueryMode="Current";

        protected override void ProcessRecord()
        {
            DisplaySettingsQueryMode d = DisplaySettingsQueryMode.QueryCurrentResolution;
            switch (QueryMode)
            {
                case "Current": d = DisplaySettingsQueryMode.QueryCurrentResolution; break;
                case "All": d = DisplaySettingsQueryMode.QueryAllSupported; break;
                case "Registry": d = DisplaySettingsQueryMode.QueryRegistryResolution; break;
            }
            foreach(DEVMODE ds in  ScreenDevice.GetDeviceResolutions(ScreenDevice.GetDesktopDeviceByName(DeviceName), d))
            {
                WriteObject(ds);
            }
        }

    }
}
