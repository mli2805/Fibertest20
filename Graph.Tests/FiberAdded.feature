Feature: FiberAdded

Background: 
	Given Левый и правый узлы созданы

@mytag
Scenario: Добавление отрезка
	When Пользователь кликает добавить отрезок
	Then Новый отрезок сохранен
