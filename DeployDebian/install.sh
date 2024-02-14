#!/bin/bash

systemctl stop rtu.service
echo "сервис остановлен"
sleep 1s

mkdir -p /var/fibertest/bin

tar -xzf rtu.tar.gz -C /var/fibertest/bin
echo "файлы распакованы"
chmod -R 777 /var/fibertest

systemctl start rtu.service
echo "сервис запущен"
sleep 2s

systemctl status rtu.service
