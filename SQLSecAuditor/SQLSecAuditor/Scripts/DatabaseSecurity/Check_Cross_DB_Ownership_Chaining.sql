/*
Description:
Opcja Cross DB Ownership Chaining steruje łańcuchowaniem własności między bazami danych na poziomie całej instancji (lub serwera).
Po włączeniu ta opcja umożliwia członkowi roli db_owner w danej bazie danych uzyskanie dostępu do obiektów, których właścicielem jest login w dowolnej innej bazie danych, co prowadzi do niepotrzebnego ujawnienia informacji.
*/
SELECT    
    name AS [Configuration name],        
    CASE        
    WHEN value_in_use = 0 THEN 'Disabled'        
    WHEN value_in_use = 1 THEN 'Enabled'    
    END AS Status
    FROM sys.configurations
    WHERE name = 'cross db ownership chaining';
/*Rationale:
When	enabled,	this	option	allows	a	member	of	the	db_owner role	in	a	database	to	gain	
access	to	objects	owned	by	a	login	in	any	other	database,	causing	an	unnecessary	
information	disclosure.	When	required,	cross-database	ownership	chaining	should	only	be	
enabled	for	the	specific	databases	requiring	it	instead	of	at	the	instance	level	for	all	
databases	by	using	the	ALTER DATABASE<database_name>SET DB_CHAINING ON command.	
This	database	option	may	not	be	changed	on	the	master,	model,	or	tempdb system	
databases.
*/