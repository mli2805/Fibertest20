Feature: EquipmentUpdated

Background: 
	Given Существует оборудование
	Given Открыта форма для изменения сущ оборудования
	Given Пользователь производит изменения

Scenario: Изменение существующего оборудования
	When Жмет сохранить
	Then Все должно быть сохранено

Scenario: Отказ от изменения существующего оборудования
	When Жмет Отмена
	# the same as in node update feature
	Then Никаких команд не подается 
