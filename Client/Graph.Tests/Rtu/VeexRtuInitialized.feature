﻿Feature: VeexRtuInitialized
Background: 
	Given На карту добавлен RTU

Scenario: Инициализация Veex RTU и создание отдельного Otau
	When Инициализируем этот RTU
	Then В списке переключателей появляется главный переключатель этого RTU
	Then В дереве у RTU квадратик переключателя зеленый
	Then В дереве у этого RTU портов - 32

	Given Создаем трассу
	And Присоединяем ее к 23 порту

	Given У RTU сломался основной переключатель
	When Переинициализируем этот RTU

	Then В дереве у RTU квадратик переключателя красный
	Then В дереве у этого RTU портов - 32
	And В событиях боп строка об аварии

	Given Ставим подменный переключатель на 16 портов
	When Переинициализируем этот RTU
	Then У переключателя изменяется id
	Then В дереве у RTU квадратик переключателя зеленый
	Then В дереве у этого RTU портов - 16
	Then В событиях боп строка об аварии пропадает
	Then Трасса становится отсоединенной