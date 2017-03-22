﻿Feature: TraceAddedIntegral

Background: 
	Given Предусловия выполнены

Scenario: Добавление трассы
	When Пользователь вводит название и коммент трассы и жмет Сохранить
	Then Трасса сохраняется
	Then Имя в дереве совпадает с именем трассы
	Then У неприсоединенной трассы есть пункты Очистить и Удалить
	Then Нет пункта Отсоединить и трех пунктов Измерения

Scenario: Отказ от создания трассы
	When Пользователь что-то вводит но жмет Отмена
	Then Трасса не сохраняется
