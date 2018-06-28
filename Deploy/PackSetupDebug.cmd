xcopy ..\Setup\bin\Debug\*.* Deploy\Pack\bin\*.* /S/D/Y
xcopy ..\Setup\LicenseDocs\*.xps Deploy\Pack\LicenseDocs\*.* /S/D/Y
      
xcopy ..\DataCenter\DataCenterService\bin\Debug\*.* Deploy\Pack\DcFiles\*.* /S/D/Y
xcopy ..\Client\WpfClient\bin\Debug\*.* Deploy\Pack\ClientFiles\*.* /S/D/Y
xcopy ..\RTU\RtuService\bin\Debug\*.* Deploy\Pack\RtuFiles\*.* /S/D/Y
xcopy ..\RTU\RtuWatchdog\bin\Debug\*.* Deploy\Pack\RtuFiles\*.* /S/D/Y
xcopy ..\RTU\OtdrMeasEngine\*.* Deploy\Pack\RtuFiles\OtdrMeasEngine\*.* /S/D/Y
pause 
xcopy ..\Uninstall\bin\Debug\*.* Deploy\Pack\UninstallFiles\*.* /S/D/Y

"C:\Program Files\WinRAR\winrar.exe" a -iiconinstall.ico -r -cfg- -sfx -z"PackSetup.conf" FtDebug_2.0.1.%1.exe Deploy\Pack\*.*
pause
