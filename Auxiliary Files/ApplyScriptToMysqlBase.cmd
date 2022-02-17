rem Потребуется ввести пароль доступа к БД - обычно root
rem Проверь путь к mysql
rem Запускать из каталога со скриптом

"c:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe" -u root -p ft20efcore < AddLastMeasurementColumn.sql
rem "c:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe" -u root -p ft20efcore < DeleteLastMeasurementColumn.sql

pause
