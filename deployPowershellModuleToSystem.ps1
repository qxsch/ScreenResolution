Param(
    [switch]
    $NoConfirmation,
    [switch]
    $InstallDebug
)

if($InstallDebug) {
    $script:sourcePath = [System.IO.Path]::Combine(
    [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition),
    "PowershellScreenManagement\bin\Debug"
    )
}
else {
    $script:sourcePath = [System.IO.Path]::Combine(
    [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition),
    "PowershellScreenManagement\bin\Release"
    )
}
$script:destinationPath = "C:\Windows\System32\WindowsPowerShell\v1.0\Modules\PowershellScreenManagement"

if( Test-Path $script:destinationPath ) {
    Write-Host -ForegroundColor Yellow "Deleting the old module"
    Remove-Item $script:destinationPath -Recurse -Confirm:(!$noConfirmation) -ErrorAction Stop -WarningAction Stop
}

if( Test-Path $script:destinationPath ) {
    Write-Host -ForegroundColor Red "Deletion of  the old module was unsuccessful! Exiting the deployment."
    exit(0)
}

if($InstallDebug) {
    Write-Host -ForegroundColor Red "Copying the new debug module."
}
else {
    Write-Host -ForegroundColor Green "Copying the new release module."
}
Copy-Item "$script:sourcePath"  $script:destinationPath -Recurse

Write-Host "You can now use the following command: Import-Module PowershellScreenManagement"