cd ..
"c:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe" /build Release Fibertest20.sln
cd Deploy\
PackSetup.cmd Release_777
pause
