rem TeamCity starts in root folder
cd Deploy\

rmdir /S/Q Pack\
rmdir /S/Q PackRtu\
rmdir /S/Q PackAdmin\
del Ft*.exe
del Ft*.zip

xcopy ..\Setup\bin\Release\*.* PackRtu\bin\*.* /S/D/Y
xcopy ..\Setup\LicenseDocs\*.xps PackRtu\LicenseDocs\*.* /S/D/Y
xcopy ..\Utils\*.* PackRtu\Utils\*.* /S/D/Y
     
xcopy ..\RTU\RtuService\bin\Release\*.* PackRtu\RtuFiles\*.* /S/D/Y
xcopy ..\RTU\RtuWatchdog\bin\Release\*.* PackRtu\RtuFiles\*.* /S/D/Y
rem OtdrMeasEngine folder will be copied from another build on TeamCity to Temp folder
xcopy Temp\OtdrMeasEngine\*.* PackRtu\RtuFiles\OtdrMeasEngine\*.* /S/D/Y
      
xcopy ..\Uninstall\bin\Release\*.* PackRtu\UninstallFiles\*.* /S/D/Y

curl --user mli:iNansIM6Y8Uq http://192.168.96.4:8989/job/windows-projects/job/RFTSViewer/pinned-for-ft20/artifact/trunk/Source/RftsReflect.zip --output PackRtu\RftsReflect.zip
cd PackRtu\
..\7z.exe x RftsReflect.zip 
del RftsReflect.zip
cd ..\

"C:\Program Files\WinRAR\winrar.exe" a -iiconinstall.ico -r -cfg- -sfx -z"PackRtuInstall.conf" FtRtu_%1.exe PackRtu\*.*
