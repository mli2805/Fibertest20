route delete 172.16.4.0
route delete 172.16.5.0
route add 172.16.5.0 mask 255.255.255.0 192.168.96.18 -p
route add 172.16.4.0 mask 255.255.255.0 172.16.4.1 -p
pause
route print
pause
