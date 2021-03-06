# ScreenResolution
Show, Update ScreenResolution with powershell or cmd.

**The key feature allows a different screen resolution for each user. The user's saved screen resolution will be automatically restored at logon time.**
Video capturing systems, require different resolutions depending on the video size, that you are broadcasting. With this feature, you can f.e. setup a broadcast user and a normal office user and always have the right screen resolution set.

[Download the installer](https://github.com/qxsch/ScreenResolution/raw/master/Binary/ScreenResolutionSetup.msi) and also see [How to Restore Resolution At Logon](https://github.com/qxsch/ScreenResolution/blob/master/Documentation/HowtoRestoreResolutionAtLogon.md)

Features:
  * c# dll to get and set the screen resolution
  * Powershell Module to:
    * get the screen resolution (current, all supported)
    * set the current screen resolution
  * Executable (UpdateScreenResolution.exe) to:
    * get the screen resolution (current, all supported)
    * set the current screen resolution
    * save current screen resolution to the registry
    * restore the screen resolution  from the registry
    * automatically restore the screen resolution at logon
    * save screen resolution to different backup profiles and restore them on demand

Developers can use deployPowershellModuleToSystem.ps1 to install the powershell module.
![Powershell Screen Management](Documentation/PowershellScreenManagement.png)

Developers can use install.ps1 to install the exe.
![UpdateScreenResolution.exe](Documentation/UpdateScreenResolutionExe.png)
