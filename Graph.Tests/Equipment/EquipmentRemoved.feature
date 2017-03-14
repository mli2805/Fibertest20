﻿Feature: EquipmentRemoved

Background: 
	Given Существует узел A с оборудованием A1
	Given Существует узел B с оборудованием B1
	Given Существуют RTU и волокна
	Given Существует трасса c оборудованием А1 в середине и B1 в конце

Scenario: Удаление оборудования не последнего для любой трассы
	Given Открыта форма для редактирования узла где оборудование А1
	Then Пункт Удалить доступен для данного оборудования
	When Пользователь нажимает удалить оборудование
	Then Оборудование удаляется из трассы
	Then Оборудование удаляется

Scenario: Попытка удаления оборудования из трассы с базовой
	Given Для этой трассы задана базовая
	Given Открыта форма для редактирования узла где оборудование А1
	Then Пункт Удалить недоступен для оборудования A1

Scenario: Попытка удаления оборудования последнего для трассы
	Given Открыта форма для редактирования узла где оборудование B1
	Then Пункт Удалить недоступен для оборудования B1

