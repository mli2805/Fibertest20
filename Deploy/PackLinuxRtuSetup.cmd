xcopy /E/D/Y ..\RtuNetCore\WatchDaemon\bin\Release\net8.0\linux-x64\publish\*.* PackLinuxRtu\
xcopy /E/D/Y ..\RtuNetCore\RtuDaemon\bin\Release\net8.0\linux-x64\publish\*.* PackLinuxRtu\

curl --user mli:iNansIM6Y8Uq http://192.168.96.4:8989/job/linux-projects/job/OtdrMeasEngine-Fibertest-linux64/pinned-for-ft30/artifact/otdrmeasengine.tar.gz --output PackLinuxRtu\otdrmeasengine.tar.gz

cd PackLinuxRtu
mkdir OtdrMeasEngine
tar -xzf otdrmeasengine.tar.gz -C OtdrMeasEngine\
del otdrmeasengine.tar.gz
cd ..

tar -C PackLinuxRtu -czf ftlinux_%1.tar.gz .
pause
