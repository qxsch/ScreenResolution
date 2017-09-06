# ScreenResolution
Show, Update ScreenResolution with powershell or cmd.

**The key feature allows a different screen resolution for each user (depending on their usage). The user's saved screen resolution will be automatically restore at logon time.**


Features:
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

Use deployPowershellModuleToSystem.ps1 to install the powershell module.
![Powershell Screen Management](Documentation/PowershellScreenManagement.png)

Use install.ps1 to install the exe.
![UpdateScreenResolution.exe](Documentation/UpdateScreenResolutionExe.png)
