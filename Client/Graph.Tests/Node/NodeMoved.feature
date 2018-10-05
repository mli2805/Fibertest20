Feature: NodeMoved

Background: 
	Given Создан узел


Scenario: Таскание узла на карте
	When Пользователь подвинул узел
	Then Новые координаты должны быть сохранены
