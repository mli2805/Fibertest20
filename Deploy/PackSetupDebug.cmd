rmdir /S/Q Pack\
del Ft*.exe
rmdir /S/Q PackWeb\
del FtWeb*.rar
rmdir /S/Q PackAdmin\
del FtAdmin*.rar

xcopy ..\Setup\bin\Debug\*.* Pack\bin\*.* /S/D/Y
xcopy ..\Setup\LicenseDocs\*.xps Pack\LicenseDocs\*.* /S/D/Y
      
xcopy ..\DataCenter\DataCenterService\bin\Debug\*.* Pack\DcFiles\*.* /S/D/Y

xcopy ..\Client\WpfClient\bin\Debug\*.* Pack\ClientFiles\*.* /S/D/Y
xcopy ..\Client\SuperClient\bin\Debug\*.* Pack\SuperClientFiles\*.* /S/D/Y

xcopy ..\RTU\RtuService\bin\Debug\*.* Pack\RtuFiles\*.* /S/D/Y
xcopy ..\RTU\RtuWatchdog\bin\Debug\*.* Pack\RtuFiles\*.* /S/D/Y

mkdir Pack\RtuFiles\OtdrMeasEngine
mkdir Pack\RtuFiles\OtdrMeasEngine\Etc
mkdir Pack\RtuFiles\OtdrMeasEngine\Etc_default
xcopy c:\VsGitProjects\Iit_otdr\__WinOut\Etc_default\*.* Pack\RtuFiles\OtdrMeasEngine\Etc\*.* /S/D/Y
xcopy c:\VsGitProjects\Iit_otdr\__WinOut\Etc_default\*.* Pack\RtuFiles\OtdrMeasEngine\Etc_default\*.* /S/D/Y
xcopy c:\VsGitProjects\Iit_otdr\__WinOut\Release_RTU\*.* Pack\RtuFiles\OtdrMeasEngine\*.* /S/D/Y
mkdir Pack\RtuFiles\OtdrMeasEngine\Share

xcopy ..\Uninstall\bin\Debug\*.* Pack\UninstallFiles\*.* /S/D/Y

rem curl --user mli:iNansIM6Y8Uq http://192.168.96.4:8989//job/RFTSViewer/lastSuccessfulBuild/artifact/trunk/Source/RftsReflect.zip --output Pack\RftsReflect.zip
rem cd Pack\
rem ..\7z.exe x RftsReflect.zip 
rem del RftsReflect.zip
rem cd ..\

"C:\Program Files\WinRAR\winrar.exe" a -iiconinstall.ico -r -cfg- -sfx -z"PackSetup.conf" FtDebug_2.0.1.%1.exe Pack\*.*
pause

xcopy ..\DataCenter\DataCenterWebApi\bin\Debug\netcoreapp3.0\*.* PackWeb\WebApiFiles\*.* /S/D/Y
xcopy ..\AngularClientApp\dist\AngularClientApp\*.* PackWeb\AngularClientApp\*.* /S/D/Y
cd PackWeb\
"C:\Program Files\WinRAR\winrar.exe" a -r ..\FtDebugWeb_2.0.1.%1.rar *.*
cd ..\
pause

xcopy ..\Client\LicenseMaker\bin\Debug\*.* PackAdmin\LicenseMaker\bin\*.* /S/D/Y
xcopy ..\Client\DbMigrationWpf\bin\Debug\*.* PackAdmin\DbMigrationWpf\bin\*.* /S/D/Y
xcopy ..\Client\KadastrLoader\bin\Debug\*.* PackAdmin\KadastrLoader\bin\*.* /S/D/Y
xcopy ..\Client\Broadcaster\bin\Debug\*.* PackAdmin\Broadcaster\bin\*.* /S/D/Y
xcopy ..\Client\MapLoader\bin\Debug\*.* PackAdmin\MapLoader\bin\*.* /S/D/Y
xcopy ..\Auxiliary Files\UserGuide\*.* PackAdmin\Auxiliary Files\UserGuide\*.* /S/D/Y
cd PackAdmin\
"C:\Program Files\WinRAR\winrar.exe" a -r ..\FtDebugAdmin_2.0.1.%1.rar *.*
cd ..\
pause

