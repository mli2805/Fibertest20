xcopy ..\Setup\bin\Debug\*.* Pack\bin\*.* /S/D/Y
xcopy ..\Setup\LicenseDocs\*.xps Pack\LicenseDocs\*.* /S/D/Y
      
xcopy ..\DataCenter\DataCenterService\bin\Debug\*.* Pack\DcFiles\*.* /S/D/Y
xcopy ..\Client\WpfClient\bin\Debug\*.* Pack\ClientFiles\*.* /S/D/Y
xcopy ..\RTU\RtuService\bin\Debug\*.* Pack\RtuFiles\*.* /S/D/Y
xcopy ..\RTU\RtuWatchdog\bin\Debug\*.* Pack\RtuFiles\*.* /S/D/Y

mkdir Pack\RtuFiles\OtdrMeasEngine
mkdir Pack\RtuFiles\OtdrMeasEngine\Etc
mkdir Pack\RtuFiles\OtdrMeasEngine\Etc_default
xcopy c:\VsGitProjects\Iit_otdr\__WinOut\Etc_default\*.* Pack\RtuFiles\OtdrMeasEngine\Etc\*.* /S/D/Y
xcopy c:\VsGitProjects\Iit_otdr\__WinOut\Etc_default\*.* Pack\RtuFiles\OtdrMeasEngine\Etc_default\*.* /S/D/Y
xcopy c:\VsGitProjects\Iit_otdr\__WinOut\Release_RTU\*.* Pack\RtuFiles\OtdrMeasEngine\*.* /S/D/Y
mkdir Pack\RtuFiles\OtdrMeasEngine\Share
pause 

xcopy ..\Uninstall\bin\Debug\*.* Pack\UninstallFiles\*.* /S/D/Y

"C:\Program Files\WinRAR\winrar.exe" a -iiconinstall.ico -r -cfg- -sfx -z"PackSetup.conf" FtDebug_2.0.1.%1.exe Pack\*.*
pause
