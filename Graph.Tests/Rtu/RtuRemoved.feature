﻿Feature: RtuRemoved

Background: 
	Given Существует РТУ
	Given Существует несколько отрезков от РТУ

Scenario: Удаление РТУ
	When Пользователь кликает на РТУ удалить
	Then РТУ удаляется
	Then Узел под РТУ и присоединенные к нему отрезки удаляются

Scenario: Попытка удаления РТУ с трассами
	Given Существует трасса от данного РТУ
	When Пользователь кликает на РТУ удалить
	Then Удаление РТУ не происходит
