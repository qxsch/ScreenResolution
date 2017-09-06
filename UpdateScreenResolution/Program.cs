using System;
using System.Collections.Generic;
using System.Linq;
using ScreenManagement;
using Mono.Options;

namespace UpdateScreenResolution
{
    class Program
    {

        static void ShowHelp()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine("get-devices");
            Console.WriteLine("\t\tGet all screen devices");
            Console.WriteLine();
            Console.WriteLine("get-resolution -d DEVICENAME [-o (current|registry|all|)]");
            Console.WriteLine("get-resolution -device=DEVICENAME [-option=(current|registry|all|)]");
            Console.WriteLine("\t\tGet the resolution of the screen device");
            Console.WriteLine();
            Console.WriteLine("set-resolution -d DEVICENAME -w WIDTH -h HEIGHT [-b BITS] [-f FREQUENCY]");
            Console.WriteLine("set-resolution -device=DEVICENAME -width=WIDTH -height=HEIGHT [-bits=BITS] [-frequency=FREQUENCY]");
            Console.WriteLine("\t\tSet the resolution of the screen device");
            Console.WriteLine();
            Console.WriteLine("save-resolution [-p PROFILENAME]");
            Console.WriteLine("save-resolution [-profile=PROFILENAME]");
            Console.WriteLine("\t\tSaves to current screen resolution settings to the user's registry for a later restore.");
            Console.WriteLine("\t\t(\"Default\" is the default profile, in case no profile has been specified.)");
            Console.WriteLine();
            Console.WriteLine("restore-resolution [-p PROFILENAME]");
            Console.WriteLine("restore-resolution [-profile=PROFILENAME]");
            Console.WriteLine("\t\tRestores the settings form the registry to the current screen resolution.");
            Console.WriteLine("\t\t(\"Default\" is the default profile, in case no profile has been specified.)");
            Console.WriteLine();
            Console.WriteLine("list-profiles");
            Console.WriteLine("\t\tList all current restore profiles.");
            Console.WriteLine();
            Console.WriteLine("delete-profile -p PROFILENAME");
            Console.WriteLine("delete-profile -profile=PROFILENAME");
            Console.WriteLine("\t\tDelete a restore profile.");
            Console.WriteLine();
            Console.WriteLine("enable-restore-at-logon [-p PROFILENAME]");
            Console.WriteLine("enable-restore-at-logon [-profile=PROFILENAME]");
            Console.WriteLine("\t\tEnables screen resolution restore at logon. (Just one profile can be activated to run.)");
            Console.WriteLine();
            Console.WriteLine("disable-restore-at-logon");
            Console.WriteLine("disable-restore-at-logon");
            Console.WriteLine("\t\tDisables screen resolution restore at logon.");
            Console.WriteLine();
        }

        static void ListDevices(string[] args)
        {
            try
            {
                foreach (DISPLAY_DEVICE d in ScreenDevice.GetDesktopDevices())
                {
                    Console.WriteLine("DEVICE \"" + d.DeviceName + "\":");
                    Console.WriteLine("    name = {0}", d.DeviceName);
                    Console.WriteLine("  string = {0}", d.DeviceString);
                    Console.WriteLine("   flags = {0}", d.StateFlags);
                    Console.WriteLine("     key = {0}", d.DeviceKey);
                    Console.WriteLine();
                }
            }
            catch (User32Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                return;
            }
        }

        static void GetResolution(string[] args)
        {
            string deviceName=null;
            DisplaySettingsQueryMode d = DisplaySettingsQueryMode.QueryCurrentResolution;

            OptionSet p = new OptionSet() {
                {
                    "d|device=", "the {NAME} of the screen device.",
                    v => deviceName = v
                },
                {
                    "o|option=", "the query {OPTION}. (all, current, registry)",
                    v => {
                        switch (v.Trim().ToLower())
                        {
                            case "current": d = DisplaySettingsQueryMode.QueryCurrentResolution; break;
                            case "all": d = DisplaySettingsQueryMode.QueryAllSupported; break;
                            case "registry": d = DisplaySettingsQueryMode.QueryRegistryResolution; break;
                        }
                    }
                }
            };
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                return;
            }

            if(deviceName == null)
            {
                Console.WriteLine("ERROR: No device name has not been specified.");
                return;
            }

            try
            {
                foreach (DEVMODE ds in ScreenDevice.GetDeviceResolutions(ScreenDevice.GetDesktopDeviceByName(deviceName), d))
                {
                    Console.WriteLine(
                       "{0} x {1}, " +
                       "{2} bit, " +
                       "{3} hertz",
                       ds.dmPelsWidth,
                       ds.dmPelsHeight,
                       ds.dmBitsPerPel,
                       ds.dmDisplayFrequency
                   );
                }
            }
            catch(User32Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                return;
            }
        }


        static void SetResolution(string[] args)
        {
            string deviceName = null;
            int width = 0;
            int height = 0;
            short bits=0;
            int frequency = 0;

            OptionSet p = new OptionSet() {
                {
                    "d|device=", "the {NAME} of the screen device.",
                    v => deviceName = v
                },
                {
                    "w|width=", "the display resolution {WIDTH} in pixels.",
                    (int v) => width = v
                },
                {
                    "h|height=", "the display resolution {HEIGHT} in pixels.",
                    (int v) => height = v
                },
                {
                    "b|bits=", "the display resolution {BITS}.",
                    (short v) => bits = v
                },
                {
                    "f|frequency=", "the display resolution {FREQUENCY} in hertz.",
                    (short v) => bits = v
                }
            };
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                return;
            }

            if (deviceName == null)
            {
                Console.WriteLine("ERROR: No device name has not been specified.");
                return;
            }

            if (width <= 0)
            {
                Console.WriteLine("ERROR: Invalid width specified.");
                return;
            }

            if (height <= 0)
            {
                Console.WriteLine("ERROR: Invalid height specified.");
                return;
            }

            try
            {
                DISPLAY_DEVICE d = ScreenDevice.GetDesktopDeviceByName(deviceName);
                if (frequency > 0 && bits > 0)
                {
                    ScreenDevice.ChangeResolution(ref d, width, height, bits, frequency);
                }
                else if (bits > 0)
                {
                    ScreenDevice.ChangeResolution(ref d, width, height, bits);
                }
                else
                {
                    ScreenDevice.ChangeResolution(ref d, width, height);
                }
            }
            catch (User32Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                return;
            }
        }

        static void SaveCurrentResolution(string[] args)
        {
            string profileName = "Default";
            bool silent = false;
            OptionSet p = new OptionSet() {
                {
                    "p|profile=", "the {PROFILENAME} of the screen resolution. (\"Default\" is the default profile.)",
                    v => profileName = RegistryHandler.sanitizeProfileName(v)
                },
                {
                    "q|quiet",  "be quiet",
                    v => silent = v != null
                }

            };
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                if (!silent) Console.WriteLine("ERROR: " + e.Message);
                return;
            }

            DisplaySet[] dsa = ScreenDevice.GetAllCurrentResolutions().ToArray();
            RegistryProfileDeviceSettings[] rpds = new RegistryProfileDeviceSettings[dsa.Length];
            int i = 0;
            foreach(DisplaySet ds in dsa)
            {
                rpds[i] = new RegistryProfileDeviceSettings
                {
                    DeviceName = ds.DisplayDevice.DeviceName,
                    Width = ds.DeviceMode.dmPelsWidth,
                    Height = ds.DeviceMode.dmPelsHeight,
                    Bits = ds.DeviceMode.dmBitsPerPel,
                    Frequency = ds.DeviceMode.dmDisplayFrequency
                };
                if(!silent) Console.WriteLine("Saving Screen resolution: " + rpds[i]);
                i++;
            }
            RegistryHandler.setProfile(profileName, rpds);
            if (!silent) Console.WriteLine("Screen resolution has been saved to profile \"" + profileName + "\".");
        }

        static void RestoreCurrentResolution(string[] args)
        {
            string profileName = "Default";
            bool silent = false;
            OptionSet p = new OptionSet() {
                {
                    "p|profile=", "the {PROFILENAME} of the screen resolution. (\"Default\" is the default profile.)",
                    v => profileName = RegistryHandler.sanitizeProfileName(v)
                },
                {
                    "q|quiet",  "be quiet",
                    v => silent = v != null
                }

            };
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                if (!silent) Console.WriteLine("ERROR: " + e.Message);
                return;
            }

            foreach (RegistryProfileDeviceSettings rpds in RegistryHandler.getProfile(profileName))
            {
                try
                {
                    if (!silent) Console.WriteLine("Restoring Screen resolution: " + rpds);
                    DISPLAY_DEVICE d = ScreenDevice.GetDesktopDeviceByName(rpds.DeviceName);
                    if (rpds.Frequency > 0 && rpds.Bits > 0)
                    {
                        try
                        {
                            ScreenDevice.ChangeResolution(ref d, rpds.Width, rpds.Height, rpds.Bits, rpds.Frequency);
                        }
                        catch(User32Exception)
                        {
                            rpds.Frequency = 0;
                            if(!silent) Console.WriteLine("Failed to change resolution, restoring without frequency: " + rpds);
                            ScreenDevice.ChangeResolution(ref d, rpds.Width, rpds.Height, rpds.Bits);
                        }
                        
                    }
                    else if (rpds.Bits > 0)
                    {
                        ScreenDevice.ChangeResolution(ref d, rpds.Width, rpds.Height, rpds.Bits);
                    }
                    else
                    {
                        ScreenDevice.ChangeResolution(ref d, rpds.Width, rpds.Height);
                    }
                }
                catch(Exception e)
                {
                    if (!silent) Console.WriteLine("ERROR: " + e.Message);
                }
            }
            if (!silent) Console.WriteLine("Screen resolution has been restored from profile \"" + profileName + "\".");
        }


        static void EnableRestoreAtLogon(string[] args)
        {
            string profileName = "Default";
            OptionSet p = new OptionSet() {
                {
                    "p|profile=", "the {PROFILENAME} of the screen resolution. (\"Default\" is the default profile.)",
                    v => profileName = RegistryHandler.sanitizeProfileName(v)
                }
            };
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                return;
            }

            string command = RegistryHandler.RegisterCurrentVersionRun(profileName);
            Console.WriteLine("Setting command = " + command);
            Console.WriteLine("Profile \"" + profileName  + "\" has been registered.");
        }

        static void DisableRestoreAtLogon(string[] args)
        {
            if(RegistryHandler.DeregisterCurrentVersionRun())
            {
                Console.WriteLine("Profile has been deregistered.");
            }
            else
            {
                Console.WriteLine("No profile has been registered, no action needed.");
            }
        }

        static void ListRegistryResolutionProfiles(string[] args)
        {
            List<string> profiles = RegistryHandler.getAllDefinedProfiles();
            if(profiles.Count > 0)
            {
                Console.WriteLine("Listing defined Profiles:");
                foreach (string profile in profiles)
                {
                    Console.WriteLine("  " + profile);
                }

            }
            else
            {
                Console.WriteLine("No profile have been defined.");
            }
        }
        
        static void DeleteRegistryResolutionProfile(string[] args)
        {
            string profileName = null;
            OptionSet p = new OptionSet() {
                {
                    "p|profile=", "the {PROFILENAME} of the screen resolution. (\"Default\" is the default profile.)",
                    v => profileName = RegistryHandler.sanitizeProfileName(v)
                }
            };
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                return;
            }

            if (profileName == null)
            {
                Console.WriteLine("ERROR: No profile name has not been specified.");
                return;
            }

            if(RegistryHandler.hasProfile(profileName))
            {
                Console.WriteLine("Deleting the profile " + profileName);
                RegistryHandler.deleteProfile(profileName);
            }
            else
            {
                Console.WriteLine("The profile " + profileName + " does not exist.");
            }
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    switch (args[0].Trim().ToLower())
                    {
                        case "get-devices": ListDevices(args.Skip(1).ToArray()); break;
                        case "get-resolution": GetResolution(args.Skip(1).ToArray()); break;
                        case "set-resolution": SetResolution(args.Skip(1).ToArray()); break;
                        case "save-resolution": SaveCurrentResolution(args.Skip(1).ToArray()); break;
                        case "restore-resolution": RestoreCurrentResolution(args.Skip(1).ToArray()); break;
                        case "list-profiles": ListRegistryResolutionProfiles(args.Skip(1).ToArray()); break;
                        case "delete-profile": DeleteRegistryResolutionProfile(args.Skip(1).ToArray()); break;
                        case "enable-restore-at-logon": EnableRestoreAtLogon(args.Skip(1).ToArray()); break;
                        case "disable-restore-at-logon": DisableRestoreAtLogon(args.Skip(1).ToArray()); break;

                        default:
                            Console.WriteLine("INVLAID COMMAND");
                            Console.WriteLine();
                            ShowHelp();
                            break;
                    }
                }
                else
                {
                    ShowHelp();
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                return;
            }
        }
    }
}
