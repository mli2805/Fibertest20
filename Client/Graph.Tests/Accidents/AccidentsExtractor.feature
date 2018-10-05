Feature: AccidentsExtractor
	

Scenario: Извлечение из sor файла мест аварий и соотнесение их с графом
	When Пришел Trace4Lm\BreakBnode2.sor
	Then Получен список эксидентов для BreakBnode2

	When Пришел Trace4Lm\BreakBnode2-MinorRnode1.sor
	Then Получен список эксидентов для BreakBnode2-MinorRnode1

	When Пришел Trace4Lm\MajorLnode2-MinorRnode1.sor
	Then Получен список эксидентов для MajorLnode2-MinorRnode1

	When Пришел Trace4Lm\MinorRnode1.sor
	Then Получен список эксидентов для MinorRnode1

	When Пришел DoubleMinorNode3.sor
	Then Получен список эксидентов для DoubleMinorNode3