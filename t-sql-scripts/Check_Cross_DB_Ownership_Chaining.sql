/*
Description:
The	cross db ownership chaining option	controls	cross-database	ownership	chaining	
across	all	databases	at	the	instance	(or	server)	level.
Rationale:
When	enabled,	this	option	allows	a	member	of	the	db_owner role	in	a	database	to	gain	
access	to	objects	owned	by	a	login	in	any	other	database,	causing	an	unnecessary	
information	disclosure.	When	required,	cross-database	ownership	chaining	should	only	be	
enabled	for	the	specific	databases	requiring	it	instead	of	at	the	instance	level	for	all	
databases	by	using	the	ALTER DATABASE<database_name>SET DB_CHAINING ON command.	
This	database	option	may	not	be	changed	on	the	master,	model,	or	tempdb system	
databases.
*/