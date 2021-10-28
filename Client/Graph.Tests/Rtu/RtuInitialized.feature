﻿Feature: RtuInitialized

Background: 
	Given Пустая база входит рут и применяет демо лицензию
	Given Существует RTU с основным 192.168.96.52 и резервным 172.16.4.10 адресами
	Given Создан еще РТУ даже с трассой

Scenario: Пользователь отказывается инициализировать РТУ
	When Пользователь открывает форму инициализации и жмет Отмена
	Then РТУ НЕ инициализирован

Scenario: Запрещено инициализировать больше RTU чем указано в лицензии
	When Пользователь вводит основной адрес 192.168.96.55 и жмет Инициализировать
	Then Выдается сообщение о превышеном лимите
	When На сервере применена другая лицензия с двумя RTU
	When Пользователь вводит основной адрес 192.168.96.55 и жмет Инициализировать
	Then RTU инициализирован только с основным адресом

Scenario: Запрещено инициализировать RTU с уже используемым адресом
	When На сервере применена другая лицензия с двумя RTU
	When Пользователь задает имя RTU 
	When Пользователь вводит основной адрес 192.168.96.52 и жмет Инициализировать
	Then Сообщение об существовании RTU с таким адресом
	When Пользователь вводит основной адрес 172.16.4.10 и жмет Инициализировать
	Then Сообщение об существовании RTU с таким адресом
	When Пользователь вводит основной 10.100.1.41 и резервный 192.168.96.52 адреса и жмет Инициализировать
	Then Сообщение об существовании RTU с таким адресом
	When Пользователь вводит основной 10.100.1.41 и резервный 172.16.4.10 адреса и жмет Инициализировать
	Then Сообщение об существовании RTU с таким адресом

	When Пользователь вводит основной адрес 192.168.96.55 и жмет Инициализировать
	Then RTU инициализирован только с основным адресом

	When Пользователь вводит основной 10.100.1.41 и резервный 10.100.12.34 адреса и жмет Инициализировать
	Then RTU инициализирован с основным и резервным адресами

