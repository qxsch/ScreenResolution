# How to automatically restore the resolution at logon?

Prerequisites: powershell >= 3.0 and .Net Framework >= 4.0

 1. Download [the installer](https://github.com/qxsch/ScreenResolution/raw/master/Binary/ScreenResolutionSetup.msi)
 2. Install at least the *application* feature
 3. start cmd (by clicking on the start menu and then type ```cmd```)
 4. go to the directory by typing: ```cd "C:\Program Files\QXSScreenResolution\"```
 5. enter the following to save the current resolution: ```.\UpdateScreenResolution.exe save-resolution```
 6. enter the following to activate restore at logon:  ```.\UpdateScreenResolution.exe enable-restore-at-logon```
 
 

# How to disable automatic resolution restore at logon?
 1. start cmd (by clicking on the start menu and then type ```cmd```)
 2. go to the directory by typing: ```cd "C:\Program Files\QXSScreenResolution\"```
 3. enter the following to activate restore at logon:  ```.\UpdateScreenResolution.exe disable-restore-at-logon```



# How to speedup the Resolution Change at logon?
Problem: The Screen resolution gets adjusted on Windows >= 8  about 11 seconds after logon.

Solution: Is described here https://www.eightforums.com/tutorials/37373-startup-delay-time-reduce-windows-8-a.html

**You have to tdo this for every user, that you want to have a faster resolution change.**

 1. Download the registry file
 2. Run the registry file by double clicking
