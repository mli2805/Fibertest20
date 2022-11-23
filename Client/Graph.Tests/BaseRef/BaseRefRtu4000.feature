﻿Feature: BaseRefRtu4000

Background:
	Given Инициализирован VeexRtu и на нем неприсоединенная трасса

Scenario: Присоединение отсоединение трассы с базовыми к порту VeexRtu
	When Пользователь присылает на сервер базовые
	When Пользователь присылает на сервер команду присоединить трассу к порту
	Then В таблице виикс-тестов появляется две записи
	When Пользователь присылает на сервер команду отсоединить трассу от порта
	Then В таблице виикс-тестов не остается записей

Scenario: Задание очистка базовых у трассы присоединенной к порту VeexRtu
	When Пользователь присылает на сервер команду присоединить трассу к порту
	When Пользователь присылает на сервер базовые
	Then В таблице виикс-тестов появляется две записи
	When Пользователь очищает базовые у трассы
	Then В таблице виикс-тестов не остается записей