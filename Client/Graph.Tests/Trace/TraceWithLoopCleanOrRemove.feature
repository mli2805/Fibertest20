Feature: TraceWithLoopCleanOrRemove
	
Background: 
	Given Существует трасса с петлей

Scenario: Очистка трассы с петлей
	When Пользователь очищает трассу
	Then Трасса удаляется

Scenario: Удаление трассы с петлей
	When Пользователь удаляет трассу
	Then Трасса удаляется
