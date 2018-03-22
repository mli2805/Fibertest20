Feature: AccidentsExtractor
	

Scenario: Извлечение из sor файла мест аварий и соотнесение их с графом
	When Пришел BreakBnode2.sor
	Then Получен список эксидентов для BreakBnode2

	When Пришел BreakBnode2-MinorRnode1.sor
	Then Получен список эксидентов для BreakBnode2-MinorRnode1

	When Пришел MajorLnode2-MinorRnode1.sor
	Then Получен список эксидентов для MajorLnode2-MinorRnode1

	When Пришел MinorRnode1.sor
	Then Получен список эксидентов для MinorRnode1