Feature: FiberRemoved

Background: 
	Given Есть два узла и отрезок между ними

Scenario: Удаление отрезка
	When Пользователь кликает удалить отрезок
	Then Отрезок удаляется
