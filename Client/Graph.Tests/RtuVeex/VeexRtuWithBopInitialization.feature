﻿Feature: VeexRtuWithBopInitialization


Scenario: Переинициализация Veex RTU с дополнительным переключателем
	Given Существует инициализированный Veex RTU с неприсоед трассой с базовыми
	When Подключаем дополн переключатель
	Then В списке переключателей есть один с заполненным VeexRtuMainOtauId для этого RTU
	And есть один переключатель второго уровня для этого RTU

	When Подключаем трассу к доп переключателю
	Then У трассы и VeexTestов Id доп переключателя

	When Переинициализируем данный RTU
	Then Id доп переключателя не меняется

	When Заменяем сломанный RTU новым без прописанного доп переключателя
	When Переинициализируем данный RTU
	Then Дочерний переключатель получает новый Id
	Then У трассы и VeexTestов Id доп переключателя