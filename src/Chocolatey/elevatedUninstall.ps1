﻿$packageName = "ServiceMatrix.VS2012"

try {

    $vs2012ToolsPath = $Env:VS110COMNTOOLS

    Write-Host "VS2012 Tools Path: $vs2012ToolsPath"

    if($vs2012ToolsPath -eq $null)
    {
    	throw "Visual Studio 2012 not found on this machine"
    }

	$vs2012Dir = New-Object System.IO.DirectoryInfo($vs2012ToolsPath)
 	$pathToVsixInstaller = [io.path]::Combine($vs2012Dir.Parent.FullName, "IDE")
 	$pathToVsixInstaller = [io.path]::Combine($pathToVsixInstaller, "VSIXInstaller.exe")
 	
 	Write-Host "Path to VsixInstaller: $pathToVsixInstaller"


	$arguments  ="/uninstall:23795EC3-3DEA-4F04-9044-4056CF91A2ED /quiet"
	
	Write-Host "Invoking vsix installer with arguments: $arguments";
    
    Start-ChocolateyProcessAsAdmin "$arguments" "$pathToVsixInstaller" -validExitCodes 0

    Write-ChocolateySuccess $packageName
} catch {
	Write-ChocolateyFailure $packageName $($_.Exception.Message)
	throw
}
