﻿Feature: Zones

Scenario: Оператор зоны
	And Добавляем Зона1
	And Добавляем Оператора для Зона1
	And Создаем RTU1 и трассу 
	And Создаем RTU2 и трассу1 и трассу2

	When Щелкаем включить трассу2 у RTU2 в Зона1
	Then У RTU2 тоже появляется птичка в столбце Зона1

	Given Сохраняем зоны
	When Перезапускаем клиентское приложение
	When Вход как Оператор для Зона1
	Then На карте видна только трасса2
	And В дереве только RTU2 трасса1 серая трасса2 синяя
