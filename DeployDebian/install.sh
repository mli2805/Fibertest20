#!/bin/bash

systemctl stop watchdog.service
echo "останавливаем сервис watchdog..."
sleep 1s
systemctl stop rtu.service
echo "останавливаем сервис rtu..."
sleep 3s

mkdir -p /var/fibertest/bin

tar -xzf bin.tar.gz -C /var/fibertest/bin
echo "файлы распакованы"
chmod -R 777 /var/fibertest

systemctl start watchdog.service
echo "запускаем сервис watchdog..."
sleep 5s

systemctl status rtu.service
