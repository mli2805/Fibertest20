﻿Feature: EquipmentUpdated

Background: 
	Given Существует оборудование
	Given Открыта форма для изменения сущ оборудования

Scenario: Изменение существующего оборудования
	When Жмет сохранить
	Given Пользователь производит изменения
	Then Все должно быть сохранено

Scenario: Отказ от изменения существующего оборудования
	When Жмет Отмена
	Given Пользователь производит изменения
	# the same as in node update feature
	Then Никаких команд не подается 
