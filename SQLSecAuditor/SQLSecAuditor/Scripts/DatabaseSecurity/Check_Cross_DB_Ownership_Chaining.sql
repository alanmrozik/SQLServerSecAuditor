/*
Description:
Cross DB Ownership Chaining controls cross-database ownership chaining at the instance level.
When enabled, a db_owner member in one database may access objects owned by the same login in another database, which can expose information unnecessarily.
*/
IF EXISTS
(
    SELECT 1
    FROM sys.configurations
    WHERE name = 'cross db ownership chaining'
      AND value_in_use = 1
)
BEGIN
    SELECT    
        name AS [Configuration name],        
        CASE        
            WHEN value_in_use = 0 THEN 'Disabled'        
            WHEN value_in_use = 1 THEN 'Enabled'    
        END AS [Status]
    FROM sys.configurations
    WHERE name = 'cross db ownership chaining';
END
ELSE
BEGIN
    SELECT 'Cross DB Ownership Chaining is disabled for all databases' AS [Status];
END;
/*Rationale:
When	enabled,	this	option	allows	a	member	of	the	db_owner role	in	a	database	to	gain	
access	to	objects	owned	by	a	login	in	any	other	database,	causing	an	unnecessary	
information	disclosure.	When	required,	cross-database	ownership	chaining	should	only	be	
enabled	for	the	specific	databases	requiring	it	instead	of	at	the	instance	level	for	all	
databases	by	using	the	ALTER DATABASE<database_name>SET DB_CHAINING ON command.	
This	database	option	may	not	be	changed	on	the	master,	model,	or	tempdb system	
databases.
*/
/*Fix:
EXECUTE sp_configure 'cross db ownership chaining', 0; 
RECONFIGURE; 
GO 
*/
