Feature: VeexRtuInitialized

Scenario: Инициализация Veex RTU и создание отдельного Otau
	Given На карту добавлен RTU
	When Инициализируем этот RTU
	Then В списке переключателей появляется главный переключатель этого RTU