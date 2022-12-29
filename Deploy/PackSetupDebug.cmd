rem TeamCity starts in root folder
cd Deploy\

rmdir /S/Q Pack\
rmdir /S/Q PackRtu\
rmdir /S/Q PackAdmin\
del Ft*.exe
del Ft*.zip

rem General installer source

xcopy ..\Install\bin\Debug\*.* Pack\bin\*.* /S/D/Y
xcopy ..\Install\UserGuide\*.pdf Pack\UserGuide\*.* /S/D/Y
     
xcopy ..\DataCenter\DataCenterService\bin\Debug\*.* Pack\DcFiles\*.* /S/D/Y
xcopy ..\DataCenter\DataCenterWebApi\bin\Debug\netcoreapp3.0\*.* Pack\WebApi\*.* /S/D/Y
xcopy ..\Client\WebClient\dist\WebClient\*.* Pack\WebClient\*.* /S/D/Y
xcopy "..\Auxiliary Files\web.config" Pack\WebApi\*.* /S/Y
xcopy "..\Auxiliary Files\*.mib" Pack\DcFiles\*.* /S/D/Y

rem echo { "apiProtocol": "protocol-placeholder", "version": "%1" } > pack\webclient\assets\settings.json

xcopy ..\Client\WpfClient\bin\Debug\*.* Pack\ClientFiles\*.* /S/D/Y
xcopy ..\Client\SuperClient\bin\Debug\*.* Pack\SuperClientFiles\*.* /S/D/Y
      
xcopy ..\Uninstall\bin\Debug\*.* Pack\UninstallFiles\*.* /S/D/Y

rem RTU installer source

xcopy ..\InstallRtu\bin\Debug\*.* PackRtu\bin\*.* /S/D/Y
     
xcopy ..\RTU\RtuService\bin\Debug\*.* PackRtu\RtuFiles\*.* /S/D/Y
xcopy ..\RTU\RtuWatchdog\bin\Debug\*.* PackRtu\RtuFiles\*.* /S/D/Y
rem OtdrMeasEngine folder will be copied from another build on TeamCity to Temp folder
xcopy Temp\OtdrMeasEngine\*.* PackRtu\RtuFiles\OtdrMeasEngine\*.* /S/D/Y
xcopy ..\Utils\*.* PackRtu\Utils\*.* /S/D/Y
 
xcopy ..\Uninstall\bin\Debug\*.* PackRtu\UninstallFiles\*.* /S/D/Y


rem get RftsReflect from Jenkins

curl --user mli:iNansIM6Y8Uq http://192.168.96.4:8989/job/windows-projects/job/RFTSViewer/pinned-for-ft20/artifact/trunk/Source/RftsReflect.zip --output Pack\RftsReflect.zip
cd Pack\
..\7z.exe x RftsReflect.zip 
del RftsReflect.zip
cd ..\

rem both installers need RftsReflect
xcopy Pack\RftsReflect\*.* PackRtu\RftsReflect\*.* /S/D/Y

"C:\Program Files\WinRAR\winrar.exe" a -iiconinstall.ico -r -cfg- -sfx -z"PackSetup.conf" Ft_%1.exe Pack\*.*
"C:\Program Files\WinRAR\winrar.exe" a -iiconinstall.ico -r -cfg- -sfx -z"PackRtuSetup.conf" FtRtu_%1.exe PackRtu\*.*


rem additional archive with administrative tools

xcopy ..\Client\Licenser\bin\Release\*.* PackAdmin\Licenser\bin\*.* /S/D/Y
xcopy ..\Client\DbMigrationWpf\bin\Debug\*.* PackAdmin\DbMigrationWpf\bin\*.* /S/D/Y
xcopy ..\Client\KadastrLoader\bin\Debug\*.* PackAdmin\KadastrLoader\bin\*.* /S/D/Y
xcopy ..\Client\Broadcaster\bin\Debug\*.* PackAdmin\Broadcaster\bin\*.* /S/D/Y
xcopy ..\Client\MapLoader\bin\Debug\*.* PackAdmin\MapLoader\bin\*.* /S/D/Y

