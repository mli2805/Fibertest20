﻿Feature: OtauDetach

Background: 
	Given Существует инициализированый RTU с неприсоединенной трассой
	Given К RTU подключен доп оптический переключатель
	Given Трасса подключена к переключателю

Scenario: Удаление переключателя запрещено если RTU в мониторинге
	Given RTU в мониторинге
	Then Пункт удаление переключателя не доступен

Scenario: Удаление оптического переключателя
	Then Пункт удаление переключателя доступен
	Given Пользователь жмет удалить оптический переключатель
	Then Трасса отсоединена от доп переключателя
	And Оптический переключатель удален
	
