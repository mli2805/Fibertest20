﻿Feature: TraceDetalization

Scenario: Запрошена детализация трассы
	Given Существует трасса с точками привязки
	When Запрошена детализация с уровнем 100
	Then Выдает 9 узлов в трассе

	When Запрошена детализация с уровнем 200
	Then Выдает 9 узлов в трассе

	When Запрошена детализация с уровнем 400
	Then Выдает 4 узлов в трассе

	When Запрошена детализация с уровнем 490
	Then Выдает 2 узлов в трассе

	When Запрошена детализация с уровнем 500
	Then Выдает 1 узлов в трассе

