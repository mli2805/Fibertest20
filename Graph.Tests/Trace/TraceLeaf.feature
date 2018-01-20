﻿Feature: TraceLeaf
	

Scenario: Отображение трассы в дереве
	Given У инициализированного RTU cоздаем трассу с названием Трасса1
	Then В дереве появляется лист с названием Трасса1 без пиктограмм
	When Задаем точную базовую
	Then Лист трассы получает ее идентификатор остальное не меняется
	When Задаем быструю базовую
	Then Лист трассы получает идентификатор быстрой остальное не меняется
	When Присоединяем трассу к порту 3
	Then Новый лист трассы на месте листа порта получает имя N3 : Трасса1 и видимые пиктограммы
	When Удаляем быструю базовую
	Then Первая пиктограмма изменяется
