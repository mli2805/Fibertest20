Feature: TraceAddedIntegral

Background: 
	Given Предусловия выполнены

Scenario: Добавление трассы
	When Пользователь вводит название и коммент трассы и жмет Сохранить
	Then Трасса сохраняется
