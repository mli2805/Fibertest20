Feature: OtauDetach

Background: 
	Given Существует инициализированый RTU с неприсоединенной трассой
	Given К RTU подключен доп оптический переключатель

Scenario: Отсоединение переключателя запрещено если к нему присоединена трасса

Scenario: Отсоединение оптического переключателя
	Given I have entered 50 into the calculator
	And I have entered 70 into the calculator
	When I press add
	Then the result should be 120 on the screen
