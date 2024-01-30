rem In VS on project RtuDaemon press publish
xcopy /E/D/Y c:\VsGitProjects\Fibertest20\RtuNetCore\RtuDaemon\bin\Release\net8.0\linux-x64\*.* rtu

rem folder OtdrMeasEngine is prepared on ubuntu2004 virtual machine
rem and place directly in /DeployDebian/rtu folder
tar -czf rtu.tar.gz rtu

rem password is iitft25user or 123
c:\putty\pscp.exe rtu.tar.gz user@192.168.96.56:/var/tmp
rem c:\putty\pscp.exe install.sh user@192.168.96.56:/var/tmp
pause