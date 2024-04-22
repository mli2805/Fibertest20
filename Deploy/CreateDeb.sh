#!/bin/bash
chmod -R 755 "./rtu_linux"
ls -l
dpkg-deb --build "./rtu_linux" "./FtRtuLinux_${Version}.deb"
chmod -R 775 "./FtRtuLinux_${Version}.deb"
