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
    [Cmdlet(VerbsCommon.Set, "ScreenResolution")]
    public class SetScreenResolutions : Cmdlet
    {
        // Declare the parameters for the cmdlet.
        [Parameter(Mandatory = true, Position = 1)]
        public string DeviceName;

        // Declare the parameters for the cmdlet.
        [Parameter(Mandatory = true, Position = 2)]
        public int Width;

        [Parameter(Mandatory = true, Position = 3)]
        public int Height;

        [Parameter(Mandatory = false, Position = 4)]
        public short? Bits;

        [Parameter(Mandatory = false, Position = 5)]
        public int? Frequency;


        protected override void ProcessRecord()
        {
            DISPLAY_DEVICE d = ScreenDevice.GetDesktopDeviceByName(DeviceName);
            if (Frequency.HasValue && Bits.HasValue)
            {
                ScreenDevice.ChangeResolution(ref d, Width, Height, Bits.Value, Frequency.Value);
            }
            else if(Bits.HasValue)
            {
                ScreenDevice.ChangeResolution(ref d, Width, Height, Bits.Value);
            }
            else
            {
                ScreenDevice.ChangeResolution(ref d, Width, Height);
            }

        }

    }
}
