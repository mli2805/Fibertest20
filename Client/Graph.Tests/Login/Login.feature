﻿Feature: Login

Scenario: Вход на пустой базе
	Given База пустая после установки или сброса
	When Рут входит и выбирает применить Демо лицензию
	Then Вход осуществлен разрешен один пользователь и один рту