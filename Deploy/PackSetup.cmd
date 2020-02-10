rem TeamCity starts in root folder
cd Deploy\

rmdir /S/Q Pack\
rmdir /S/Q PackRtu\
rmdir /S/Q PackAdmin\
del Ft*.exe
del Ft*.zip

rem General installer source

xcopy ..\Install\bin\Release\*.* Pack\bin\*.* /S/D/Y
xcopy ..\Install\LicenseDocs\*.xps Pack\LicenseDocs\*.* /S/D/Y
     
xcopy ..\DataCenter\DataCenterService\bin\Release\*.* Pack\DcFiles\*.* /S/D/Y
xcopy ..\DataCenter\DataCenterWebApi\bin\Release\netcoreapp3.0\*.* Pack\WebApi\*.* /S/D/Y
xcopy ..\Client\WebClient\dist\WebClient\*.* Pack\WebClient\*.* /S/D/Y
xcopy "..\Auxiliary Files\web.config" Pack\WebApi\*.* /S/Y
xcopy "..\Auxiliary Files\*.mib" Pack\DcFiles\*.* /S/D/Y

xcopy ..\Client\WpfClient\bin\Release\*.* Pack\ClientFiles\*.* /S/D/Y
xcopy ..\Client\SuperClient\bin\Release\*.* Pack\SuperClientFiles\*.* /S/D/Y
      
xcopy ..\Uninstall\bin\Release\*.* Pack\UninstallFiles\*.* /S/D/Y

rem RTU installer source

xcopy ..\InstallRtu\bin\Release\*.* PackRtu\bin\*.* /S/D/Y
xcopy ..\InstallRtu\LicenseDocs\*.xps PackRtu\LicenseDocs\*.* /S/D/Y
     
xcopy ..\RTU\RtuService\bin\Release\*.* PackRtu\RtuFiles\*.* /S/D/Y
xcopy ..\RTU\RtuWatchdog\bin\Release\*.* PackRtu\RtuFiles\*.* /S/D/Y
rem OtdrMeasEngine folder will be copied from another build on TeamCity to Temp folder
xcopy Temp\OtdrMeasEngine\*.* PackRtu\RtuFiles\OtdrMeasEngine\*.* /S/D/Y
xcopy ..\Utils\*.* PackRtu\Utils\*.* /S/D/Y
 
xcopy ..\Uninstall\bin\Release\*.* PackRtu\UninstallFiles\*.* /S/D/Y


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

xcopy ..\Client\LicenseMaker\bin\Release\*.* PackAdmin\LicenseMaker\bin\*.* /S/D/Y
xcopy ..\Client\DbMigrationWpf\bin\Release\*.* PackAdmin\DbMigrationWpf\bin\*.* /S/D/Y
xcopy ..\Client\KadastrLoader\bin\Release\*.* PackAdmin\KadastrLoader\bin\*.* /S/D/Y
xcopy ..\Client\Broadcaster\bin\Release\*.* PackAdmin\Broadcaster\bin\*.* /S/D/Y
xcopy ..\Client\MapLoader\bin\Release\*.* PackAdmin\MapLoader\bin\*.* /S/D/Y

