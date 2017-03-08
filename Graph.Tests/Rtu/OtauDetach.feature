﻿Feature: OtauDetach

Background: 
	Given Существует инициализированый RTU с неприсоединенной трассой
	Given К RTU подключен доп оптический переключатель

Scenario: Удаление переключателя запрещено если к нему присоединена трасса
	Given Трасса подключена к переключателю
	Then Пункт удаление переключателя недоступен

Scenario: Удаление оптического переключателя
	Given Пользователь жмет удалить оптический переключатель
	Then Оптический переключатель удален
