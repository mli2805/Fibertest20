Feature: RtuUpdated
	
Background:
	Given Ранее был создан RTU с именем blah-blah
	Given Добавлен RTU

Scenario: Сохранение без изменений
	When Пользователь открыл окно редактирования первого RTU и ничего не изменив нажал Сохранить
	Then Команд не подается

Scenario: Сохранение изменений
	Given Пользователь ввел название нового RTU node-node
	Then Сохраняется название RTU node-node

Scenario: Сохранение изменений комментария
	Given Пользователь ввел комментарий к RTU some comment
	Then Сохраняется комментарий RTU some comment

Scenario: Сохранение с существующим именем RTU
	Given Пользователь ввел название нового RTU blah-blah
	Then Будет сигнализация ошибки

Scenario: Отказ от изменения RTU
	When Пользователь открыл окно редактирования RTU и что-то изменив нажал Отменить
	Then Команд не подается
	