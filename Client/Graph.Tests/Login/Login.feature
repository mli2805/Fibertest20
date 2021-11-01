﻿Feature: Login

Background: 
	Given База пустая после установки или сброса

Scenario: Вход на пустой базе с Демо лицензией
	When Рут входит и выбирает применить Демо лицензию
	Then Вход осуществлен разрешен один пользователь и один рту
	Then Рут выходит на этом же компе входит оператор

Scenario: Вход на пустой базе с лицензией без привязки рабмест
	When Рут входит и указывает файл с лицензией без привязки рабмест
	Then Вход осуществлен пользователи и рту разрешены в соответствии с лицензионным файлом
	Then Рут выходит на этом же компе входит оператор


Scenario: Вход на пустой базе с лицензией с привязкой рабмест
	When Рут входит и указывает файл с лицензией с привязкой рабмест вводится пароль безопасника
	Then Вход осуществлен Рут привязан к машине Пользователи и рту разрешены кроме вэбклиентов 
	When Рут выходит
	And Оператор входит Выдается запрос пароля безопасника Неверный пароль безопасника
	Then Не пускает
	When Верный пароль безопасника 
	Then Успешно входит
	When Оператор выходит
	Then Рут входит Пароль безопасника больше не спрашивает
	When Рут применяет основную лицензию без привязки
	Then Старая лицензия удалена Новая лицензия применена 
	And При входе супервизора пароль безопасника не требует


