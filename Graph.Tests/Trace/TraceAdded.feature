﻿Feature: TraceAdded

Background: 
	Given Существует набор узлов и отрезков

Scenario: Добавление без имени трассы
	Given Пользователь выбрал узел где есть оборудование
	Then На вопрос: "Accept the path?" собираются ответить: "Yes"
	Then На предложение выбрать оборудование собираются ответить: "Use"
	Given И кликнул определить трассу
	Then Открывается окно добавления трассы
	When Пользователь НЕ вводит имя трассы и жмет Сохранить
	Then Окно не закрывается
	Then Трасса не сохраняется

Scenario: В последний момент пользователь решает не создавать трассу
	Given Пользователь выбрал узел где есть оборудование
	Then На вопрос: "Accept the path?" собираются ответить: "Yes"
	Given И кликнул определить трассу
	Then Открывается окно добавления трассы
	When Пользователь жмет Отмена
	Then Окно закрывается
	Then Трасса не сохраняется

Scenario: Добавление трассы
	Given И кликнул определить трассу
	Then Открывается окно добавления трассы
	When Пользователь вводит название трассы и жмет Сохранить
	Then Новая трасса сохраняется и окно закрывается

