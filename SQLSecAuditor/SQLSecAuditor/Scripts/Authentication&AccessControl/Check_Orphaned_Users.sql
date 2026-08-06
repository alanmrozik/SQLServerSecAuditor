/*
Description:
A	database	user	for	which	the	corresponding	SQL	Server	login	is	undefined	or	is	incorrectly	
defined	on	a	server	instance	cannot	log	in	to	the	instance	and	is	referred	to	as	orphaned	
and	should	be	removed.
*/
--USE <database_name>;
--GO
EXEC sp_change_users_login @Action='Report';
/*Rationale:
Orphan	users	should	be	removed	to	avoid	potential	misuse	of	those	broken	users	in	any	
way.
*/