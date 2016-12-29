Feature: NodeMoved

Background: 
	Given Создан узел


Scenario: Узел подвинут на карте
	When Пользователь подвинул узел
	Then Новые координаты должны быть сохранены
