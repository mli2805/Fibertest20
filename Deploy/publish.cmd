dotnet publish -c Release -r linux-x64 ..\RtuNetCore\RtuDaemon\RtuDaemon.csproj
dotnet publish -c Release -r linux-x64 ..\RtuNetCore\WatchDaemon\WatchDaemon.csproj

rem result is in RtuNetCore\RtuDaemon\bin\Release\net8.0\linux-x64\publish\

pause
