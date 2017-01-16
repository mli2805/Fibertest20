﻿Feature: NodeUpdated

Background:
	Given Ранее был создан узел с именем blah-blah
	Given Добавлен узел
	Given Открыто окно для изменения данного узла

# TODO: what is the title initialized with?



Scenario: Сохранение без изменений
	When Нажата клавиша сохранить
	Then Никаких команд не подается

Scenario: Сохранение изменений
	Given Пользователь ввел название узла node-node
	When Нажата клавиша сохранить
	Then Измененный узел сохраняется

Scenario: Сохранение изменений комментария
	Given Пользователь ввел какой-то комментарий к узлу
	When Нажата клавиша сохранить
	Then Измененный узел сохраняется

Scenario: Сохранение с существующим именем узла
	Given Пользователь ввел название узла blah-blah
	When Нажата клавиша сохранить
	Then Некая сигнализация ошибки

Scenario: Отказ от изменения узла
	When Нажата клавиша отменить
	Then Никаких команд не подается
	