/*
Description:
The	Ole Automation Procedures option	controls	whether	OLE	Automation	objects	can	be	
instantiated	within	Transact-SQL batches.	These	are	extended	stored	procedures	that	allow	
SQL	Server	users	to	execute	functions	external	to	SQL	Server.
*/
SELECT 
    name as [Name], 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'Ole Automation Procedures';
    /*
    Rationale:
Enabling	this	option	will	increase	the	attack	surface	of	SQL	Server	and	allow	users	to	
execute	functions	in	the	security	context	of	SQL	Server*/