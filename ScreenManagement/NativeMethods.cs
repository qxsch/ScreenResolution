using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ScreenManagement
{
    public class DisplaySet
    {
        public readonly DISPLAY_DEVICE DisplayDevice;
        public readonly DEVMODE DeviceMode;
        public DisplaySet(DISPLAY_DEVICE DisplayDevice, DEVMODE DeviceMode)
        {
            this.DisplayDevice = DisplayDevice;
            this.DeviceMode = DeviceMode;
        }
    }

    public class User32Exception : Exception
    {
        public User32Exception(string message) : base(message) { }
        public User32Exception(string message, Exception innerException) : base(message, innerException) { }
    }


    [Flags()]
    public enum DisplayDeviceStateFlags : int
    {
        // from: http://www.pinvoke.net/default.aspx/Enums/DisplayDeviceStateFlags.html
        // equvalent to defines from: wingdi.h (c:\Program Files (x86)\Windows Kits\10\Include\10.0.10240.0\um\wingdi.h)
        //#define DISPLAY_DEVICE_ATTACHED_TO_DESKTOP      0x00000001
        //#define DISPLAY_DEVICE_MULTI_DRIVER             0x00000002
        //#define DISPLAY_DEVICE_PRIMARY_DEVICE           0x00000004
        //#define DISPLAY_DEVICE_MIRRORING_DRIVER         0x00000008
        //#define DISPLAY_DEVICE_VGA_COMPATIBLE           0x00000010
        //#if (_WIN32_WINNT >= _WIN32_WINNT_WIN2K)
        //#define DISPLAY_DEVICE_REMOVABLE                0x00000020
        //#endif // (_WIN32_WINNT >= _WIN32_WINNT_WIN2K)
        //#if (_WIN32_WINNT >= _WIN32_WINNT_WIN8)
        //#define DISPLAY_DEVICE_ACC_DRIVER               0x00000040
        //#endif
        //#define DISPLAY_DEVICE_MODESPRUNED              0x08000000
        //#if (_WIN32_WINNT >= _WIN32_WINNT_WIN2K)
        //#define DISPLAY_DEVICE_REMOTE                   0x04000000
        //#define DISPLAY_DEVICE_DISCONNECT               0x02000000
        //#endif
        //#define DISPLAY_DEVICE_TS_COMPATIBLE            0x00200000
        //#if (_WIN32_WINNT >= _WIN32_WINNT_LONGHORN)
        //#define DISPLAY_DEVICE_UNSAFE_MODES_ON          0x00080000
        //#endif

        ///* Child device state */
        //#if (_WIN32_WINNT >= _WIN32_WINNT_WIN2K)
        //#define DISPLAY_DEVICE_ACTIVE              0x00000001
        //#define DISPLAY_DEVICE_ATTACHED            0x00000002
        //#endif // (_WIN32_WINNT >= _WIN32_WINNT_WIN2K)
        /// <summary>The device is part of the desktop.</summary>
        AttachedToDesktop = 0x1,
        MultiDriver = 0x2,
        /// <summary>The device is part of the desktop.</summary>
        PrimaryDevice = 0x4,
        /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
        MirroringDriver = 0x8,
        /// <summary>The device is VGA compatible.</summary>
        VGACompatible = 0x10,
        /// <summary>The device is removable; it cannot be the primary display.</summary>
        Removable = 0x20,
        /// <summary>The device has more display modes than its output devices support.</summary>
        ModesPruned = 0x8000000,
        Remote = 0x4000000,
        Disconnect = 0x2000000,

        /// <summary>Child device state: DISPLAY_DEVICE_ACTIVE</summary>
        Active = 0x1,
        /// <summary>Child device state: DISPLAY_DEVICE_ATTACHED</summary>
        Attached = 0x2
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        [MarshalAs(UnmanagedType.U4)]
        public DisplayDeviceStateFlags StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    //DEVMODE structure, see http://msdn.microsoft.com/en-us/library/ms535771(VS.85).aspx
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    public enum DisplayChangeResultCode
    {
        DISP_CHANGE_SUCCESSFUL = 0,
        DISP_CHANGE_RESTART = 1,
        DISP_CHANGE_FAILED = -1,
        DISP_CHANGE_BADMODE = -2,
        DISP_CHANGE_NOTUPDATED = -3,
        DISP_CHANGE_BADFLAGS = -4,
        DISP_CHANGE_BADPARAM = -5,
        DISP_CHANGE_BADDUALVIEW = -6
    }

  
    public enum DisplaySettingsQueryMode
    {
        QueryAllSupported=1,
        QueryCurrentResolution=2,
        QueryRegistryResolution=3
    }

    internal class User32
    {
        // https://retep998.github.io/doc/winapi/winuser/constant.ENUM_REGISTRY_SETTINGS.html
        public const uint ENUM_REGISTRY_SETTINGS = 0xFFFFFFFE;
        // https://retep998.github.io/doc/winapi/winuser/constant.ENUM_CURRENT_SETTINGS.html
        public const uint ENUM_CURRENT_SETTINGS = 0xFFFFFFFF;


        // PInvoke declaration for EnumDisplaySettings Win32 API
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, uint iModeNum, ref DEVMODE lpDevMode);

        // PInvoke declaration for ChangeDisplaySettings Win32 API
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern int ChangeDisplaySettings(ref DEVMODE lpDevMode, int dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern bool EnumDisplayDevices(string lpDevice, int iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, int dwFlags);
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern int ChangeDisplaySettingsEx(string lpDeviceName, ref DEVMODE lpDevMode, IntPtr HWND, int dwflags, IntPtr lParam);
    }

    public class ScreenDevice
    {

        private List<DISPLAY_DEVICE> _DisplayDevices = new List<DISPLAY_DEVICE>();

        private List<DISPLAY_DEVICE> _Monitors = new List<DISPLAY_DEVICE>();
        public List<DISPLAY_DEVICE> Monitors
        {
            get { return _Monitors; }
        }

        public List<DISPLAY_DEVICE> DisplayDevices
        {
            get { return _DisplayDevices; }
        }

        private static DEVMODE CreateDevMode()
        {
            DEVMODE dm = new DEVMODE();
            dm.dmDeviceName = new string(new char[33]);
            dm.dmFormName = new string(new char[33]);
            dm.dmSize = Convert.ToInt16(Marshal.SizeOf(dm));
            return dm;
        }

        private static DISPLAY_DEVICE CreateDisplayDevice()
        {
            DISPLAY_DEVICE dd = new DISPLAY_DEVICE();
            dd.cb = System.Runtime.InteropServices.Marshal.SizeOf(typeof(DISPLAY_DEVICE));
            dd.DeviceName = new string(new char[33]);
            dd.DeviceString = new string(new char[129]);
            dd.DeviceID = new string(new char[129]);
            dd.DeviceKey = new string(new char[129]);
            return dd;
        }


        #region "DevMode Flags"
        //Flags for DevMode structure, see http://msdn.microsoft.com/en-us/library/ms535771(VS.85).aspx


        public const int DM_ORIENTATION = 0x1;

        public const int DM_PAPERSIZE = 0x2;

        public const int DM_PAPERLENGTH = 0x4;

        public const int DM_PAPERWIDTH = 0x8;

        public const int DM_SCALE = 0x10;

        public const int DM_POSITION = 0x20;

        public const int DM_NUP = 0x40;

        public const int DM_DISPLAYORIENTATION = 0x80;

        public const int DM_COPIES = 0x100;

        public const int DM_DEFAULTSOURCE = 0x200;

        public const int DM_PRINTQUALITY = 0x400;

        public const int DM_COLOR = 0x800;

        public const int DM_DUPLEX = 0x1000;

        public const int DM_YRESOLUTION = 0x2000;

        public const int DM_TTOPTION = 0x4000;

        public const int DM_COLLATE = 0x8000;

        public const int DM_FORMNAME = 0x10000;

        public const int DM_LOGPIXELS = 0x20000;

        public const int DM_BITSPERPEL = 0x40000;

        public const int DM_PELSWIDTH = 0x80000;

        public const int DM_PELSHEIGHT = 0x100000;

        public const int DM_DISPLAYFLAGS = 0x200000;

        public const int DM_DISPLAYFREQUENCY = 0x400000;

        public const int DM_ICMMETHOD = 0x800000;

        public const int DM_ICMINTENT = 0x1000000;

        public const int DM_MEDIATYPE = 0x2000000;

        public const int DM_DITHERTYPE = 0x4000000;

        public const int DM_PANNINGWIDTH = 0x8000000;

        public const int DM_PANNINGHEIGHT = 0x10000000;
        #endregion
        public const int DM_DISPLAYFIXEDOUTPUT = 0x20000000;

        public const int CDS_UPDATEREGISTRY = 0x1;
        public const int CDS_FULLSCREEN = 0x4;
        public const int CDS_NORESET = 0x10000000;
        public const int CDS_SET_PRIMARY = 0x10;

        public const int CDS_RESET = 0x40000000;
        

        //Lots of different ways to change resolution
        //Remark: The device parameter is always the list(of DISPLAY_DEVICE) DisplayDevices

        public static void ChangeResolution(ref DISPLAY_DEVICE device, int width, int height)
        {
            DEVMODE DevM = CreateDevMode();
            bool enumResult = false;
            DisplayChangeResultCode changeResult = default(DisplayChangeResultCode);

            DevM.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT;

            
            enumResult = User32.EnumDisplaySettings(device.DeviceName, User32.ENUM_CURRENT_SETTINGS, ref DevM);

            DevM.dmPelsWidth = width;
            DevM.dmPelsHeight = height;

            DevM.dmDeviceName = device.DeviceName;

            changeResult = (DisplayChangeResultCode)User32.ChangeDisplaySettingsEx(device.DeviceName, ref DevM, IntPtr.Zero, (0), IntPtr.Zero);

            // faild to change, due to a bad mode? try again with a better mode
            if(changeResult == DisplayChangeResultCode.DISP_CHANGE_BADMODE)
            {
                DevM = FindBestSupportedDeviceMode(device, DevM);
                changeResult = (DisplayChangeResultCode)User32.ChangeDisplaySettingsEx(device.DeviceName, ref DevM, IntPtr.Zero, (0), IntPtr.Zero);
            }

            if (changeResult != DisplayChangeResultCode.DISP_CHANGE_SUCCESSFUL)
            {
                throw new User32Exception("Failed to change resolution: " + changeResult.ToString());
            }


        }

        public static void ChangeResolution(ref DISPLAY_DEVICE device, int width, int height, short bits)
        {
            DEVMODE DevM = CreateDevMode();
            bool enumResult = false;
            DisplayChangeResultCode changeResult = default(DisplayChangeResultCode);

            DevM.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_BITSPERPEL;


            enumResult = User32.EnumDisplaySettings(device.DeviceName, User32.ENUM_CURRENT_SETTINGS, ref DevM);

            DevM.dmPelsWidth = width;
            DevM.dmPelsHeight = height;
            DevM.dmBitsPerPel = bits;

            DevM.dmDeviceName = device.DeviceName;

            changeResult = (DisplayChangeResultCode)User32.ChangeDisplaySettingsEx(device.DeviceName, ref DevM, IntPtr.Zero, (0), IntPtr.Zero);

            // faild to change, due to a bad mode? try again with a better mode
            if (changeResult == DisplayChangeResultCode.DISP_CHANGE_BADMODE)
            {
                DevM = FindBestSupportedDeviceMode(device, DevM);
                changeResult = (DisplayChangeResultCode)User32.ChangeDisplaySettingsEx(device.DeviceName, ref DevM, IntPtr.Zero, (0), IntPtr.Zero);
            }

            if (changeResult != DisplayChangeResultCode.DISP_CHANGE_SUCCESSFUL)
            {
                throw new User32Exception("Failed to change resolution: " + changeResult.ToString());
            }


        }


        public static void ChangeResolution(ref DISPLAY_DEVICE device, int width, int height, short bits, int freq)
        {
            DEVMODE DevM = CreateDevMode();
            bool enumResult = false;
            DisplayChangeResultCode changeResult = default(DisplayChangeResultCode);

            DevM.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_BITSPERPEL | DM_DISPLAYFREQUENCY;

            enumResult = User32.EnumDisplaySettings(device.DeviceName, User32.ENUM_CURRENT_SETTINGS, ref DevM);

            DevM.dmPelsWidth = width;
            DevM.dmPelsHeight = height;
            DevM.dmBitsPerPel = bits;
            DevM.dmDisplayFrequency = freq;

            DevM.dmDeviceName = device.DeviceName;

            changeResult = (DisplayChangeResultCode)User32.ChangeDisplaySettingsEx(device.DeviceName, ref DevM, IntPtr.Zero, (0), IntPtr.Zero);

            if (changeResult != DisplayChangeResultCode.DISP_CHANGE_SUCCESSFUL)
            {
                throw new User32Exception("Failed to change resolution: " + changeResult.ToString());
            }

        }
        /*public static void ChangeResolution(List<DISPLAY_DEVICE> devices)
        {
            DEVMODE DevM1 = CreateDevMode();
            DEVMODE DevM2 = CreateDevMode();
            bool enumResult = false;
            DisplayChangeResultCode changeResult = default(DisplayChangeResultCode);

            DevM1.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY | DM_POSITION;

            enumResult = User32.EnumDisplaySettings(devices[0].DeviceName, User32.ENUM_CURRENT_SETTINGS, ref DevM1);

            DevM1.dmPelsWidth = 1024;
            DevM1.dmPelsHeight = 768;
            DevM1.dmDisplayFrequency = 50;

            DevM1.dmPositionX = -1024;
            DevM1.dmPositionY = 0;

            DevM1.dmDeviceName = devices[0].DeviceName;


            DevM2.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY | DM_POSITION;

            enumResult = User32.EnumDisplaySettings(devices[1].DeviceName, User32.ENUM_CURRENT_SETTINGS, ref DevM2);


            DevM2.dmPelsWidth = 800;
            DevM2.dmPelsHeight = 600;
            DevM2.dmDisplayFrequency = 50;

            DevM2.dmPositionX = 0;
            DevM2.dmPositionY = 0;

            DevM2.dmDeviceName = devices[1].DeviceName;

            changeResult = (DisplayChangeResultCode)User32.ChangeDisplaySettingsEx(devices[0].DeviceName, ref DevM1, IntPtr.Zero, (0), IntPtr.Zero);

            if (changeResult != DisplayChangeResultCode.DISP_CHANGE_SUCCESSFUL)
            {
                throw new User32Exception("Failed to change resolution: " + changeResult.ToString());
            }

            changeResult = (DisplayChangeResultCode)User32.ChangeDisplaySettingsEx(devices[1].DeviceName, ref DevM2, IntPtr.Zero, (CDS_SET_PRIMARY), IntPtr.Zero);

            if (changeResult != DisplayChangeResultCode.DISP_CHANGE_SUCCESSFUL)
            {
                throw new User32Exception("Failed to change resolution: " + changeResult.ToString());
            }

            DEVMODE d = new DEVMODE();
            d.dmSize = (short)Marshal.SizeOf(d);
            changeResult = (DisplayChangeResultCode)User32.ChangeDisplaySettingsEx(null, ref d, IntPtr.Zero, 0, IntPtr.Zero);

            if (changeResult != DisplayChangeResultCode.DISP_CHANGE_SUCCESSFUL)
            {
                throw new User32Exception("Failed to change resolution: " + changeResult.ToString());
            }
        }*/

        public static void ChangeResolution(ref DISPLAY_DEVICE device, int width, int height, int freq, int PositionX, int PositionY)
        {
            DEVMODE DevM = CreateDevMode();
            bool enumResult = false;
            DisplayChangeResultCode changeResult = default(DisplayChangeResultCode);

            DevM.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY | DM_POSITION;

            enumResult = User32.EnumDisplaySettings(device.DeviceName, User32.ENUM_CURRENT_SETTINGS, ref DevM);

            DevM.dmPelsWidth = width;
            DevM.dmPelsHeight = height;
            DevM.dmDisplayFrequency = freq;

            DevM.dmPositionX = PositionX;
            DevM.dmPositionY = PositionY;

            DevM.dmDeviceName = device.DeviceName;

            changeResult = (DisplayChangeResultCode)User32.ChangeDisplaySettings(ref DevM, 0);

            if (changeResult != DisplayChangeResultCode.DISP_CHANGE_SUCCESSFUL)
            {
                throw new User32Exception("Failed to change resolution: " + changeResult.ToString());
            }

        }


        public static List<DISPLAY_DEVICE> GetAllDevices()
        {
            List<DISPLAY_DEVICE> devices = new List<DISPLAY_DEVICE>();

            DISPLAY_DEVICE device = new DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE));
            for (int devId = 0; User32.EnumDisplayDevices(null, devId, ref device, 0); devId++)
            {
                devices.Add(device);
            }

            return devices;
        }

        public static DISPLAY_DEVICE GetDesktopDeviceByName(string deviceName)
        {
            foreach (DISPLAY_DEVICE dev in GetDesktopDevices())
            {
                if(dev.DeviceName==deviceName)
                {
                    return dev;
                }
            }
            throw new User32Exception("No display found for name \"" + deviceName + "\"");
        }

        public static List<DISPLAY_DEVICE> GetDesktopDevices()
        {
            List<DISPLAY_DEVICE> devices = new List<DISPLAY_DEVICE>();

            DISPLAY_DEVICE device = new DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE));
            for (int devId = 0; User32.EnumDisplayDevices(null, devId, ref device, 0); devId++)
            {
                if (device.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop))
                {
                    devices.Add(device);
                }

            }

            return devices;
        }

        public static List<DEVMODE> GetDeviceResolutions(DISPLAY_DEVICE device, DisplaySettingsQueryMode queryMode)
        {
            List<DEVMODE> devModes = new List<DEVMODE>();
            try
            {
                DEVMODE mode = new DEVMODE();
                switch (queryMode)
                {
                    case DisplaySettingsQueryMode.QueryCurrentResolution:
                        if(User32.EnumDisplaySettings(device.DeviceName, User32.ENUM_CURRENT_SETTINGS, ref mode))
                        {
                            devModes.Add(mode);
                        }
                        break;

                    case DisplaySettingsQueryMode.QueryAllSupported:
                        for (uint i = 0; User32.EnumDisplaySettings(device.DeviceName, i, ref mode); i++)
                        {
                            devModes.Add(mode);
                        }
                        break;

                    case DisplaySettingsQueryMode.QueryRegistryResolution:
                        if (User32.EnumDisplaySettings(device.DeviceName, User32.ENUM_REGISTRY_SETTINGS, ref mode))
                        {
                            devModes.Add(mode);
                        }
                        break;
                }
            }
            catch { }
            return devModes;
        }

        public static List<DisplaySet> GetAllCurrentResolutions()
        {
            List<DisplaySet> devicesAndModes = new List<DisplaySet>();

            foreach (DISPLAY_DEVICE dev in GetDesktopDevices())
            {
                foreach(DEVMODE mode in GetDeviceResolutions(dev, DisplaySettingsQueryMode.QueryCurrentResolution))
                {
                    devicesAndModes.Add(new DisplaySet(dev, mode));
                }
            }

            return devicesAndModes;
        }

        public static List<DisplaySet> GetAllSupportedResolutions()
        {
            List<DisplaySet> devicesAndModes = new List<DisplaySet>();

            foreach (DISPLAY_DEVICE dev in GetDesktopDevices())
            {
                foreach (DEVMODE mode in GetDeviceResolutions(dev, DisplaySettingsQueryMode.QueryAllSupported))
                {
                    devicesAndModes.Add(new DisplaySet(dev, mode));
                }
            }

            return devicesAndModes;
        }

        public static DEVMODE FindBestSupportedDeviceMode(DISPLAY_DEVICE device, DEVMODE mode)
        {
            DEVMODE bestDevice = new DEVMODE();
            bool bestDeviceFound = false;

            foreach (DEVMODE supportedMode in GetDeviceResolutions(device, DisplaySettingsQueryMode.QueryAllSupported))
            {
                if(mode.dmPelsWidth == supportedMode.dmPelsWidth && mode.dmPelsHeight == supportedMode.dmPelsHeight && mode.dmBitsPerPel == supportedMode.dmBitsPerPel)
                {
                    bestDeviceFound = true;
                    // better frequency
                    if(Math.Abs(mode.dmDisplayFrequency - supportedMode.dmDisplayFrequency) <
                       Math.Abs(mode.dmDisplayFrequency - bestDevice.dmDisplayFrequency))
                    {
                        bestDevice = supportedMode;
                    }
                }
            }

            if(!bestDeviceFound)
            {
                throw new User32Exception("No supported device mode has been found.");
            }


            return bestDevice;
        }

    }
}