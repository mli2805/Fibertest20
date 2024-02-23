dotnet publish ..\RtuNetCore\RtuDaemon\RtuDaemon.csproj --configuration=Release --runtime=linux-x64
xcopy /E/D/Y c:\VsGitProjects\Fibertest20\RtuNetCore\RtuDaemon\bin\Release\net8.0\linux-x64\publish\*.* bin

rem if published in Visual Studio
rem xcopy /E/D/Y c:\VsGitProjects\Fibertest20\RtuNetCore\RtuDaemon\bin\Release\net8.0\linux-x64\*.* bin


rem folder OtdrMeasEngine is prepared on ubuntu2004 virtual machine
rem and place directly in /DeployDebian/rtu folder
tar -C bin -czf bin.tar.gz .

rem 172.16.4.209 / 192.168.96.56 / 
rem password is iitft25user or 123
c:\putty\pscp.exe bin.tar.gz user@192.168.96.56:/var/tmp
rem c:\putty\pscp.exe install.sh user@192.168.96.56:/var/tmp


rem c:\putty\pscp.exe Rtu.service user@192.168.96.56:/var/tmp
rem c:\putty\pscp.exe setrtu.sh user@192.168.96.56:/var/tmp


rem 192.168.96.199 (debian virtualbox)
rem c:\putty\pscp.exe rtu.tar.gz leanid@192.168.96.199:/var/tmp
rem c:\putty\pscp.exe install.sh leanid@192.168.96.199:/var/tmp
pause