﻿Feature: RtuAddedAtGpsLocation
	
Scenario: Добавление РТУ
	Given На сервере применена демо лицензия с одним RTU
	When Пользователь кликает добавить РТУ
	Then Новый РТУ сохраняется

	When Пользователь кликает добавить РТУ
	Then Выдается сообщение о превышеном лимите

	When На сервере применена другая лицензия с двумя RTU
	When Пользователь кликает добавить РТУ
	Then Новый РТУ сохраняется
