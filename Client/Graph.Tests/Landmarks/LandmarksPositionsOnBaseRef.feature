Feature: LandmarksPositionsOnBaseRef

A short summary of the feature

@tag1
Scenario: Изменение положения ориентиров при задании физ длины участка
	Given Задана трасса с базовой
	When Пользователь задает физ длину участка 100
	Then Ориентиры сдвигаются
