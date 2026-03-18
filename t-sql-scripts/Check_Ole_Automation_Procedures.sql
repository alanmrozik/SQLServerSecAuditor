/*
Description:
The	Ole Automation Procedures option	controls	whether	OLE	Automation	objects	can	be	
instantiated	within	Transact-SQL batches.	These	are	extended	stored	procedures	that	allow	
SQL	Server	users	to	execute	functions	external	to	SQL	Server.
Rationale:
Enabling	this	option	will	increase	the	attack	surface	of	SQL	Server	and	allow	users	to	
execute	functions	in	the	security	context	of	SQL	Server
*/
SELECT name, 
 CAST(value as int) as value_configured, 
 CAST(value_in_use as int) as value_in_use 
FROM sys.configurations 
WHERE name = 'Ole Automation Procedures'; 