﻿Feature: MonitoringSettingsViewModel

Scenario: Форма настроек мониторинга
	Given Создан RTU
	Then Пункт Настройки мониторинга недоступен
	Given RTU успешно инициализирован
	And И к порту 2 подключеной дополнительный переключатель
	And Создана трасса подключена к 3 порту RTU и заданы базовые
	And Еще трасса только с точной базовой подключена к 4 порту БОПа

	When Пользователь открывает Настройки мониторинга - вторая трасса недоступна для включения в цикл мониторинга
	When Пользователь включает автоматический режим и жмет применить
	Then Сообщение что не задана ни одна трасса для мониторинга

	Given Задаем второй трассе быструю базовую
	Then Теперь вторую трассу разрешено включить в цикл мониторинга
	And По умолчанию частота сохранения и точных и быстрых - Не сохранять

	When Пользователь ставит птичку включить все трассы главного переключателя
	And Птичку включить именно вторую трассу на четвертом порту БОПа
	Then Длительность точных базовых обоих трасс включена в длительность цикла мониторинга

	Given Пользователь ставит частоту измерения и сохранения точной 6 часов
	When Пользователь уменьшает частоту измерения по точной
	Then Частота сохранения по точной изменяется соответственно
	When Пользователь включает авто режим и жмет применить

	When Пользователь жмет секретную комбинацию Ctrl-B для пересылки базовых на RTU