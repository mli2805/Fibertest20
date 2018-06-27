xcopy Setup\bin\Debug\*.* Pack\bin\*.* /S/D/Y
xcopy Setup\LicenseDocs\*.xps Pack\LicenseDocs\*.* /S/D/Y

xcopy DataCenter\DataCenterService\bin\Debug\*.* Pack\DcFiles\*.* /S/D/Y
xcopy Client\WpfClient\bin\Debug\*.* Pack\ClientFiles\*.* /S/D/Y
xcopy RTU\RtuService\bin\Debug\*.* Pack\RtuFiles\*.* /S/D/Y
xcopy RTU\RtuWatchdog\bin\Debug\*.* Pack\RtuFiles\*.* /S/D/Y
;xcopy OtdrMeasEngine\*.* Pack\RtuFiles\OtdrMeasEngine\*.* /S/D/Y
pause
xcopy Uninstall\bin\Debug\*.* Pack\UninstallFiles\*.* /S/D/Y

"C:\Program Files\WinRAR\rar.exe" a -r -cfg- -sfx -z"PackSetup.conf" -iiconinstall.ico FtSetupDebug_2.0.1.%1.exe Pack\*.*
pause