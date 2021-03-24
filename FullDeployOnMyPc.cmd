rem Start this cmd with fullVersion as a parameter

VersionPatcher.exe %1

cd Client\WebClient\
rem ng build --prod --output-hashing=all
cd ..\..

"c:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\devenv.exe" /build Release Fibertest25.sln

Deploy\PackSetup.cmd %1
pause 