#!/bin/bash

systemctl stop Rtu.service
echo "сервис остановлен"
sleep 1s

tar -xzf rtu.tar.gz -C /var/fibertest
echo "файлы распакованы"
chmod -R 777 /var/fibertest

systemctl start Rtu.service
echo "сервис запущен"
sleep 2s

systemctl status Rtu.service
