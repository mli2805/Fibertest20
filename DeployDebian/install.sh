#!/bin/bash

systemctl stop rtu.service
echo "останавливаем сервис..."
sleep 3s

mkdir -p /var/fibertest/bin

tar -xzf bin.tar.gz -C /var/fibertest/bin
echo "файлы распакованы"
chmod -R 777 /var/fibertest

systemctl start rtu.service
echo "запускаем сервис..."
sleep 3s

systemctl status rtu.service
