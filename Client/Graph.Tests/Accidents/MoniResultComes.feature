Feature: MoniResultComes
	
Scenario: Приход результата моноторинга с аварией
	Given Трасса c 4 ориентирами на мониторинге

	When Приходит Fast FiberBreak с файлом Trace4lm\BreakBnode2
	Then Все красное и Крест совпадающий с муфтой

	When Приходит Precise FiberBreak с файлом Trace4lm\BreakBnode2
	Then Все красное и Крест совпадающий с муфтой

	When Приходит Precise Major с файлом Trace4lm\MajorLnode2-MinorRnode1
	Then Все фиолетовое и Кресты совпадающие с проключением и муфтой

	#авария как в первый раз
	When Приходит Precise FiberBreak с файлом Trace4lm\BreakBnode2
	Then Все красное и Крест совпадающий с муфтой

	When Приходит Precise Ok с файлом Trace4lm\4lm-Ok
	Then Отображается что все ОК

	When Приходит Precise Major с файлом Trace4lm\MajorLnode2-MinorRnode1
	Then Все фиолетовое и Кресты совпадающие с проключением и муфтой

	When Отсоединяем трассу от порта
	Then Все синее и никаких крестов

	When Добавляем на трассе точки привязки

	When Присоединяем трассу к любому порту
	# так как было перед отсоединением
	Then Снова трасса фиолетовая и Кресты совпадающие с проключением и муфтой
