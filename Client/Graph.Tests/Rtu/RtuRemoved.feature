﻿Feature: RtuRemoved

Background: 
	Given Существует один RTU
	Given Существуют еще RTU несколько узлов и отрезки между ними
	Given Существует трасса от первого РТУ
	Given Существует трасса от второго РТУ последние отрезки трасс совпадают

Scenario: Удаление РТУ с неприсоединенной трассой (на карте)
	When Пользователь кликает удалить первый RTU
	Then Трасса удаляемого RTU не удаляется но очищается
	Then РТУ удаляется
	Then Узел под РТУ и присоединенные к нему отрезки удаляются

Scenario: Удаление РТУ с неприсоединенной трассой (в дереве)
	When Пользователь кликает на первом RTU в дереве удалить
	Then Трасса удаляемого RTU не удаляется но очищается
	Then РТУ удаляется
	Then Узел под РТУ и присоединенные к нему отрезки удаляются

Scenario: Запрещено удаление РТУ с присоединенной к порту трассой
	Given Трасса присоединенна к порту РТУ
	Then У РТУ на карте пункт меню Удалить недоступен
	Then У РТУ в дереве пункт меню Удалить недоступен