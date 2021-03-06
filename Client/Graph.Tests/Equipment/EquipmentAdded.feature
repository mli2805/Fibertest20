﻿Feature: EquipmentAdded

Background: 
	Given Дан узел с оборудованием
	Given Еще есть РТУ другие узлы и волокна
	Given Одна трасса заканчивается в данном узле
	Given Одна трасса проходит через данный узел и использует оборудование
	Given Одна трасса проходит через данный узел но не использует оборудование

Scenario: Добавление в узел трассы но не в трассу
	Then Пользователь не отмечает ни одну трассу для включения оборудования
	Then Пользователь вводит парамы оборудования и жмет Сохранить
	Then В окне Добавить оборудование
	Then В узле создается новое оборудование
	Then Но в трассы оно не входит

Scenario: Добавление в трассы
	Then Пользователь отмечает все трассы для включения оборудования
	Then Пользователь вводит парамы оборудования и жмет Сохранить
	Then В окне Добавить оборудование
	Then В узле создается новое оборудование
	Then Новое оборудование входит во все трассы а старое ни в одну


Scenario: Добавление в трассу с заданной базовой недоступно
	Given Для одной из трасс задана базовая
	Then На форме выбора трасс эта трасса недоступна для выбора остальные доступны

Scenario: Отказ от добавления оборудования
	Then Пользователь отмечает все трассы для включения оборудования
	Then Пользователь вводит парамы оборудования и жмет Отмена
	Then В окне Добавить оборудование
	Then В узле НЕ создается новое оборудование
