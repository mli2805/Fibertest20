Feature: AccidentsExtractor
	

Scenario: Извлечение из sor файла мест аварий и соотнесение их с графом
	Given Включен мониторинг трассы

	When Пришел MoniResult2.sor
	Then Получен список эксидентов

	When Пришел MoniResult1.sor
	Then Получен другой список эксидентов