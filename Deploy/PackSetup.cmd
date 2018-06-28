rem TeamCity starts in root folder
cd Deploy\

rmdir /S/Q Pack\
del Ft*.exe

xcopy ..\Setup\bin\Release\*.* Pack\bin\*.* /S/D/Y
xcopy ..\Setup\LicenseDocs\*.xps Pack\LicenseDocs\*.* /S/D/Y
     
xcopy ..\DataCenter\DataCenterService\bin\Release\*.* Pack\DcFiles\*.* /S/D/Y
xcopy ..\Client\WpfClient\bin\Release\*.* Pack\ClientFiles\*.* /S/D/Y
xcopy ..\RTU\RtuService\bin\Release\*.* Pack\RtuFiles\*.* /S/D/Y
xcopy ..\RTU\RtuWatchdog\bin\Release\*.* Pack\RtuFiles\*.* /S/D/Y
rem OtdrMeasEngine folder will be copied from another build on TeamCity to Temp folder
xcopy Temp\OtdrMeasEngine\*.* Pack\RtuFiles\OtdrMeasEngine\*.* /S/D/Y
      
xcopy ..\Uninstall\bin\Release\*.* Pack\UninstallFiles\*.* /S/D/Y

"C:\Program Files\WinRAR\winrar.exe" a -iiconinstall.ico -r -cfg- -sfx -z"PackSetup.conf" Ft_%1.exe Pack\*.*

