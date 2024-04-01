#!/bin/bash

set -e

echo -n "Version: "

read version

chmod -R 775 ./rtu_linux/

rm -rf ./rtu_linux/ft/var/fibertest/bin/*

tar -xzf ftlinux_$version.tar.gz -C ./rtu_linux/ft/var/fibertest/bin/ 

sed -i -- "2 s/.*/Version: $version/" "./rtu_linux/ft/DEBIAN/control" # запись номера версии в файл control

#chmod -R 775 /var/tmp/rtu_linux/

echo "Файлы Распакованы"

# далее команды для сборки deb-пакета
version1=$(awk 'NR==2 {print $2}' "./rtu_linux/ft/DEBIAN/control") # читаем номер версии из файла conrtol (вторая строчка, второе слово) 

echo "The Package Name:  ---rtulinux_$version1.deb---"

sleep 3s

dpkg-deb --build "./rtu_linux/ft" "./rtulinux_$version1.deb"

chmod -R 777 "./rtulinux_$version1.deb"

echo " ---The Build has comlited---"

