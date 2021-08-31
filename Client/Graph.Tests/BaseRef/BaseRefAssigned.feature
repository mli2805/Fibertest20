﻿Feature: BaseRefAssigned

Background: 
	Given Была создана трасса
	Then Пункт Задать базовые недоступен 
	When RTU успешно инициализируется c длинной волны SM1625
	Then Пункт Задать базовые становится доступен

Scenario: Сохранение и очистка базовых
	When Пользователь указывает пути к точной и быстрой базовам и жмет сохранить
	Then У трассы заданы точная и быстрая базовые
	When Пользователь изменяет быструю и жмет сохранить
	Then У трассы старая точная и новая быстрая базовые
	When Пользователь сбрасывает точную и задает дополнительную и жмет сохранить
	Then Выдается сообщение что точная должна быть задана
	When Пользователь задает дополнительную и жмет сохранить
	Then У трассы задана дополнительная


Scenario: Сохранение имен узлов и оборудования в базовой рефлектограмме
	Given Задается имя Последний узел для узла с оконечным кроссом
	Given Оконечный кросс меняется на Другое и имя оборудования Др-1
	When Пользователь указывает пути к точной и быстрой базовам и жмет сохранить
	Then У сохраненных на сервере базовых третий ориентир имеет имя Последний узел / Др-1 и тип Другое