@{
    GUID = "{8D3DB5F5-C1A1-42F7-9F81-DFBBE635BCED}"
    Author = "Marco Weber"
    CompanyName = "QXS.CH"
    Copyright = "© QXS.CH. All rights reserved."
    ModuleVersion = "1.0"
    PowerShellVersion = "3.0"
    ClrVersion = "4.0"
    NestedModules = "PowershellScreenManagement.dll"
    TypesToProcess = @()
    FormatsToProcess = @()
    CmdletsToExport = @(
        'Get-ScreenDevices',
        'Get-ScreenResolution',
        'Set-ScreenResolution'
    )
    AliasesToExport = @()
}