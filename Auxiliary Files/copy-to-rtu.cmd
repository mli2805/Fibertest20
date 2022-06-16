xcopy m:\rtu\rtuservice\bin\debug\*.* c:\iit-fibertest\rtumanager\bin\*.* /D /Y /S /EXCLUDE:c:\excludedfileslist.txt
xcopy m:\rtu\rtuwatchdog\bin\debug\*.* c:\iit-fibertest\rtumanager\bin\*.* /D /Y /EXCLUDE:c:\excludedfileslist.txt
pause
