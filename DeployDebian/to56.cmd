dotnet publish ..\RtuNetCore\RtuDaemon\RtuDaemon.csproj --configuration=Release --runtime=linux-x64
dotnet publish ..\RtuNetCore\WatchDaemon\WatchDaemon.csproj --configuration=Release --runtime=linux-x64
xcopy /E/D/Y c:\VsGitProjects\Fibertest20\RtuNetCore\WatchDaemon\bin\Release\net8.0\linux-x64\publish\*.* bin
xcopy /E/D/Y c:\VsGitProjects\Fibertest20\RtuNetCore\RtuDaemon\bin\Release\net8.0\linux-x64\publish\*.* bin

rem xcopy /E/D/Y c:\VsGitProjects\Fibertest20\RtuNetCore\WatchDaemon\bin\Release\net8.0\linux-x64\publish\Iit.Fibertest.WatchDaemon bin
rem xcopy /E/D/Y c:\VsGitProjects\Fibertest20\RtuNetCore\WatchDaemon\bin\Release\net8.0\linux-x64\publish\Iit.Fibertest.WatchDaemon.* bin


rem if published in Visual Studio
rem xcopy /E/D/Y c:\VsGitProjects\Fibertest20\RtuNetCore\RtuDaemon\bin\Release\net8.0\linux-x64\*.* bin


set address="192.168.96.56"
rem set address="172.16.4.209"%


rem folder OtdrMeasEngine is prepared on ubuntu2004 virtual machine
rem and place directly in /DeployDebian/rtu folder
tar -C bin -czf bin.tar.gz .

rem password is iitft25user or 123
c:\putty\pscp.exe bin.tar.gz user@%address%:/var/tmp
rem c:\putty\pscp.exe install.sh user@192.168.96.56:/var/tmp
rem c:\putty\pscp.exe rtu.service user@192.168.96.56:/var/tmp
rem c:\putty\pscp.exe watchdog.service user@192.168.96.56:/var/tmp
rem c:\putty\pscp.exe setrtu.sh user@192.168.96.56:/var/tmp

rem to copy folder with files
rem c:\putty\pscp.exe -r charon-reset user@192.168.96.56:/var/tmp
pause