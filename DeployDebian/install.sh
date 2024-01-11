#!/bin/bash

systemctl stop Rtu.service
echo "сервис остановлен"
sleep 1s

tar -xzf rtu.tar.gz -C /var/fibertest
echo "файлы распакованы"

systemctl start Rtu.service
echo "сервис запущен"
sleep 2s

systemctl status Rtu.service
