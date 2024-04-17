#!/bin/bash
dpkg-deb --build "./rtu_linux" "./rtulinux_$1.deb"
chmod -R 777 "./rtulinux_$1.deb"

