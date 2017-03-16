Feature: GpsInputModeChanged


Scenario: Изменение формата отображения GPS координат
	Given Широта 53,047817959114688
	And I have entered 70 into the calculator
	When I press add
	Then the result should be 120 on the screen
