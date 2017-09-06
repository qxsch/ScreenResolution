$destPath = "C:\Program Files\UpdateScreenResolution"
$srcPath = [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition), "UpdateScreenResolution\bin\Release")

if(Test-Path $destPath) {
    Remove-Item -Recurse $destPath
}

Write-Host "Creating the path $destPath"
New-Item -ItemType directory -Path $destPath | Out-Null

Write-Host "Copying from $srcPath    to  $destPath"
Copy-Item -Recurse "$srcPath\*" $destPath -Exclude "*.pdb"
