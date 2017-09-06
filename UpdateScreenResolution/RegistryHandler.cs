using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

namespace UpdateScreenResolution
{
    [Serializable]
    public class RegistryProfileDeviceSettings
    {
        public string DeviceName;
        public int Width = 0;
        public int Height = 0;
        public short Bits = 0;
        public int Frequency = 0;

        public override string ToString()
        {
            string s = "DeviceName = " + DeviceName + ", Width = " + Width + ", Height = " + Height;
            if(Bits > 0)
            {
                s += ", Bits = " + Bits;
            }
            if (Frequency > 0)
            {
                s += ", Frequency = " + Frequency;
            }
            return s;
        }
    }

    public class RegistryHandler
    {
        const string SoftwareVendor = "QXS";
        const string SoftwareName = "UpdateScreenResolution";

        public static string sanitizeProfileName(string profileName)
        {
            if (profileName.Length > 20) profileName = profileName.Substring(0, 20);
            return profileName;
        }

        public static List<string> getAllDefinedProfiles()
        {
            List<string> profiles = new List<string>();
            createSoftwareRegistryKeys();
            using (RegistryKey SoftwareNameKey = Registry.CurrentUser.OpenSubKey("Software\\" + SoftwareVendor + "\\" + SoftwareName, false))
            {
                foreach(string valueName in SoftwareNameKey.GetValueNames())
                {
                    if(valueName.StartsWith("Profile"))
                    {
                        profiles.Add(valueName.Substring(7));
                    }
                }
            }

            return profiles;
        }

        public static bool hasProfile(string profileName)
        {
            profileName = sanitizeProfileName(profileName);
            createSoftwareRegistryKeys();
            using (RegistryKey SoftwareNameKey = Registry.CurrentUser.OpenSubKey("Software\\" + SoftwareVendor + "\\" + SoftwareName, false))
            {
                try
                {
                    return SoftwareNameKey.GetValueKind("Profile" + profileName) == RegistryValueKind.Binary;
                }
                catch (IOException) { return false; }
            }
        }

        public static RegistryProfileDeviceSettings[] getProfile(string profileName)
        {
            profileName = sanitizeProfileName(profileName);
            RegistryProfileDeviceSettings[] settings = new RegistryProfileDeviceSettings[] { };
            createSoftwareRegistryKeys();
            using (RegistryKey SoftwareNameKey = Registry.CurrentUser.OpenSubKey("Software\\" + SoftwareVendor + "\\" + SoftwareName, false))
            {
                try
                {
                    if (SoftwareNameKey.GetValueKind("Profile" + profileName) != RegistryValueKind.Binary)
                    {
                        throw new RegistryHandlerException("Failed to load the profile \"" + profileName + "\", because it is not a binary entry.");
                    }
                }
                catch (IOException e) { throw new RegistryHandlerException("The profile \"" + profileName + "\" does not exist in the registry.", e); }
                
                using (MemoryStream memStream = new MemoryStream((byte[])SoftwareNameKey.GetValue("Profile" + profileName, new byte[] { })))
                {
                    if(memStream.Length <= 1)
                    {
                        throw new RegistryHandlerException("Failed to load the profile \"" + profileName + "\", because it is not a valid profile serialization.");
                    }
                    BinaryFormatter formatter = new BinaryFormatter();
                    settings  = (RegistryProfileDeviceSettings[]) formatter.Deserialize(memStream);
                }
            }
            return settings;
        }

        public static void deleteProfile(string profileName)
        {
            setProfile(profileName, new RegistryProfileDeviceSettings[] { });
        }

        public static void setProfile(string profileName, RegistryProfileDeviceSettings[] settings)
        {
            profileName = sanitizeProfileName(profileName);
            createSoftwareRegistryKeys();
            using (RegistryKey SoftwareNameKey = Registry.CurrentUser.OpenSubKey("Software\\" + SoftwareVendor + "\\" + SoftwareName, true))
            {
                if (settings.Length >= 1)
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(memStream, settings);
                        SoftwareNameKey.SetValue("Profile" + profileName, memStream.ToArray(), RegistryValueKind.Binary);
                    }
                }
                else
                {
                    SoftwareNameKey.DeleteValue("Profile" + profileName);
                }
                SoftwareNameKey.Close();
            }
        }

        public static void createSoftwareRegistryKeys()
        {
            RegistryKey SoftwareKey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey SoftwareVendorKey = null;
            RegistryKey SoftwareNameKey = null;
            try
            {
                SoftwareVendorKey = Registry.CurrentUser.OpenSubKey("Software\\QXS", true);
                if (SoftwareVendorKey == null)
                {
                    SoftwareKey.CreateSubKey(SoftwareVendor);
                    SoftwareVendorKey = Registry.CurrentUser.OpenSubKey("Software\\" + SoftwareVendor, true);
                }

                if (SoftwareVendorKey == null)
                {
                    throw new Exception("FAILED TO CREATE THE REGISTRY KEY: HKEY_CURRENT_USER\\" + "Software\\" + SoftwareVendor);
                }

                SoftwareNameKey = Registry.CurrentUser.OpenSubKey("Software\\" + SoftwareVendor + "\\" + SoftwareName, true);
                if (SoftwareNameKey == null)
                {
                    SoftwareVendorKey.CreateSubKey(SoftwareName);
                    SoftwareNameKey = Registry.CurrentUser.OpenSubKey("Software\\" + SoftwareVendor + "\\" + SoftwareName, false);
                }

                if (SoftwareNameKey == null)
                {
                    throw new Exception("FAILED TO CREATE THE REGISTRY KEY: HKEY_CURRENT_USER\\" + "Software\\" + SoftwareVendor + "\\" + SoftwareName);
                }
            }
            finally
            {
                if (SoftwareNameKey != null)
                {
                    SoftwareVendorKey.Close();
                    SoftwareNameKey.Dispose();
                }

                if (SoftwareVendorKey != null)
                {
                    SoftwareVendorKey.Close();
                    SoftwareVendorKey.Dispose();
                }

                if (SoftwareKey != null)
                {
                    SoftwareKey.Close();
                    SoftwareVendorKey.Dispose();
                }

            }

        }

        public static string RegisterCurrentVersionRun(string profileName)
        {
            profileName = sanitizeProfileName(profileName);
            if(!hasProfile(profileName))
            {
                throw new RegistryHandlerException("The profile \"" + profileName + "\" does not exist.");
            }

            string command = "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\" restore-resolution -p \"" + profileName + "\" -q";
            using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                try
                {
                    runKey.GetValueKind(SoftwareVendor + " " + SoftwareName);
                    runKey.DeleteValue(SoftwareVendor + " " + SoftwareName);
                }
                catch(IOException) { }
                runKey.SetValue(SoftwareVendor + " " + SoftwareName, command);

                runKey.Close();
            }
            return command;
        }

        public static bool DeregisterCurrentVersionRun()
        {
            bool deleted = false;
            using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                try
                {
                    runKey.GetValueKind(SoftwareVendor + " " + SoftwareName);
                    runKey.DeleteValue(SoftwareVendor + " " + SoftwareName);
                    deleted = true;
                }
                catch (IOException) { }

                runKey.Close();
            }
            return deleted;
        }
    }


}
