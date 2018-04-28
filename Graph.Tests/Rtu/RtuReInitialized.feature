﻿Feature: RtuReInitialized
	
Scenario: Переинициализация RTU в связи с подменой
	Given Существует RTU с адресом 192.168.96.58 с длиной волны SM1625 и с 16 портами
	And БОП подключен к порту RTU 13
	And Трасса подключена к порту RTU 4
	And Трасса подключена к порту БОПа 2
	When RTU заменен на старинный не поддерживающий БОПы RTU
	And Пользователь жмет переинициализировать RTU
	Then Выдается сообщение что подключение БОПов не поддерживается
	Then В дереве ничего не меняется

	When RTU заменяется на свежий c 8 портами
	And Пользователь жмет переинициализировать RTU
	Then Выдается сообщение что слишком большой номер порта
	Then В дереве ничего не меняется