Feature: RftsEventsViewModel
	
Scenario: Форма рефлектометрических событий
	When Открываем форму событий для измерения CriticalRnode5-MinorLnode3-MinorCnode3
	Then Форма имеет 3 вкладки
	Then На всех вкладках по 7 событий
	And Состояние трассы Critical
	And Все пороги состояние плохо

	When Открываем форму событий для измерения Trace4Lm\NoFiber
	Then Нет вкладок есть только надпись Нет волокна

	When Открываем форму событий для измерения Trace5Lm\BreakBbetween2and3
	Then Форма имеет 3 вкладки
	And Состояние трассы fiber break
	And Все пороги состояние плохо
