rem In VS on project RtuDaemon press publish
xcopy /E/D/Y c:\VsGitProjects\Fibertest20\RtuNet6\RtuDaemon\bin\Release\net6.0\linux-x64\*.* rtu

rem folder OtdrMeasEngine is prepared on ubuntu2004 virtual machine
tar -czf rtu.tar.gz rtu

rem password is iitft25user
c:\putty\pscp.exe rtu.tar.gz user@192.168.96.56:/var/tmp
rem c:\putty\pscp.exe install.sh user@192.168.96.56:/var/tmp
pause