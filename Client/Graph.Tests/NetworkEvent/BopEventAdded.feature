﻿Feature: BopEventAdded
	
Scenario: Приход сообщений о исправности БОПа
	Given Есть инициализированный RTU
	When Пользователь присоединяет OTAU с адресом 2.2.2.2 11834
	Then У OTAU зеленый квадрат и у RTU на месте для БОПа зеленый квадрат

	When Приходит сообщение OTAU 123456 НЕисправен	
	Then Событие отражается в обеих таблицах на вкладке сетевых событий БОП
	Then Открывается форма Состояние БОП - в поле состояние - авария
	Then В дереве у RTU для БОПа красный квадрат и у самого OTAU красный квадрат

	When Приходит сообщение OTAU 123456 исправен
	Then В актуальных аварийное событие пропадает а во всех появляется событие ок
	Then На форме Состояние БОП в поле состояние становится ОК
	Then У OTAU зеленый квадрат и у RTU на месте для БОПа зеленый квадрат
