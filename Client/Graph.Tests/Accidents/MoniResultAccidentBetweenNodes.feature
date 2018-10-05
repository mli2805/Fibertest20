Feature: MoniResultAccidentBetweenNodes

Scenario: Приход результатов мониторинга для трассы с аварией между узлами

	Given Трасса с 5 ориентирами на мониторинге

	When Приходи результат Fast FiberBreak с файлом Trace5lm\BreakBbetween2and3
	Then Все красное и Крест между муфтой и проключением
