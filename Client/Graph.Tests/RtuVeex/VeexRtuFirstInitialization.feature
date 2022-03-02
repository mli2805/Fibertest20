Feature: VeexRtuFirstInitialization
Background: 
	Given На карте есть RTU

Scenario: Начальная инициализация Veex RTU с поломанным Otau
	Given У RTU один поломанный основной переключатель
	When Первая инициализация этого RTU
	Then В дереве у RTU портов - 1

Scenario: Начальная инициализация Veex RTU с 3 недоступными Otau
	Given У RTU три недоступных основных переключателя
	When Первая инициализация этого RTU
	Then В дереве у RTU портов - 1
	
Scenario: Начальная инициализация Veex RTU без Otau
	Given У RTU нет основного переключателя
	When Первая инициализация этого RTU
	Then В дереве у RTU портов - 1
	
Scenario: Начальная инициализация Veex RTU с несколькими основными Otau
	Given У RTU три основных переключателя и только один с восьмью портами доступен
	When Первая инициализация этого RTU
	Then В дереве у RTU портов - 8

Scenario: Начальная инициализация Veex RTU с основным и доп Otau
	Given У RTU основной на 32 и доп на 16 портов переключатели
	When Первая инициализация этого RTU
	Then В дереве у RTU портов - 32
