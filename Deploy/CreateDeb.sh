#!/bin/bash
chmod -R 755 "./rtu_linux"
dpkg-deb --build "./rtu_linux" "./FtRtuLinux_${Version}.deb"
chmod -R 777 "./FtRtuLinux_${Version}.deb"
