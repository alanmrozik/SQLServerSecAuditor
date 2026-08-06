/*
Description:
Użytkownik bazy danych, dla którego odpowiadający mu login programu SQL Server jest niezdefiniowany lub niepoprawnie zdefiniowany w instancji serwera, nie może zalogować się do tej instancji - jest on określany mianem "użytkownika osieroconego" i powinien zostać usunięty.
*/
--USE <database_name>;
--GO
EXEC sp_change_users_login @Action='Report';
/*Rationale:
Orphan	users	should	be	removed	to	avoid	potential	misuse	of	those	broken	users	in	any	
way.
*/