﻿Feature: RtuUpdatedFromTree
	
Scenario: Клик в дереве открывает окно позволяет редактировать и сохранять изменения RTU
	Given Существует RTU
	When Пользователь кликает Информация в меню RTU что-то вводит и жмет сохранить
	Then Изменения применяются

