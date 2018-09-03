rem TeamCity starts in root folder
cd Deploy\

rmdir /S/Q Pack\
del Ft*.exe
rmdir /S/Q PackAdmin\
del FtAdmin*.exe

xcopy ..\Setup\bin\Release\*.* Pack\bin\*.* /S/D/Y
xcopy ..\Setup\LicenseDocs\*.xps Pack\LicenseDocs\*.* /S/D/Y
     
xcopy ..\DataCenter\DataCenterService\bin\Release\*.* Pack\DcFiles\*.* /S/D/Y

xcopy ..\Client\WpfClient\bin\Release\*.* Pack\ClientFiles\*.* /S/D/Y
xcopy ..\Client\SuperClient\bin\Release\*.* Pack\SuperClientFiles\*.* /S/D/Y

xcopy ..\RTU\RtuService\bin\Release\*.* Pack\RtuFiles\*.* /S/D/Y
xcopy ..\RTU\RtuWatchdog\bin\Release\*.* Pack\RtuFiles\*.* /S/D/Y
rem OtdrMeasEngine folder will be copied from another build on TeamCity to Temp folder
xcopy Temp\OtdrMeasEngine\*.* Pack\RtuFiles\OtdrMeasEngine\*.* /S/D/Y
      
xcopy ..\Uninstall\bin\Release\*.* Pack\UninstallFiles\*.* /S/D/Y

curl --user mli:iNansIM6Y8Uq http://192.168.96.8:8888/job/RFTSViewer/lastSuccessfulBuild/artifact/trunk/Source/RftsReflect.zip --output Pack\RftsReflect.zip
cd Pack\
..\7z.exe x RftsReflect.zip 
del RftsReflect.zip
cd ..\

"C:\Program Files\WinRAR\winrar.exe" a -iiconinstall.ico -r -cfg- -sfx -z"PackSetup.conf" Ft_%1.exe Pack\*.*

xcopy ..\Client\LicenseMaker\bin\Release\*.* PackAdmin\LicenseMaker\bin\*.* /S/D/Y
xcopy ..\Client\DbMigrationWpf\bin\Release\*.* PackAdmin\DbMigrationWpf\bin\*.* /S/D/Y
xcopy ..\Client\KadastrLoader\bin\Release\*.* PackAdmin\KadastrLoader\bin\*.* /S/D/Y
cd PackAdmin\
"C:\Program Files\WinRAR\winrar.exe" a -r ..\FtAdmin_%1.rar *.*
cd ..\