﻿Feature: BaseRefModified

	
Scenario: Добавление ориентиров при задании базовой
	Given Существует инициализированный RTU
	And К нему нарисована трасса1 с пустыми узлами
	When Пользователь задает рефлектограмму BaseRef1
	Then На рефлектограмму добавляются ориентиры соответствующие пустым узлам Но расстояние до ориентиров оборудования не изменяется

	When Пользователь добавляет точки привязки и двигает их
	And Пользователь задает рефлектограмму BaseRef1
	Then При применении базовой ориентиры пустые узлы оказываются на другом расстоянии

	When Пользователь добавляет кабельный резерв в пустой узел после проверяемого
	And Пользователь задает рефлектограмму BaseRef1
	Then При применении базовой ориентиры для пустых узлов снова сдвигаются
