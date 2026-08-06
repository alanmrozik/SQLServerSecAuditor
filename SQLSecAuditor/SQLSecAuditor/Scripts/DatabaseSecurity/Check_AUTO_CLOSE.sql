/*
Description:
AUTO_CLOSE determines	if	a	given	database	is	closed	or	not	after	a	connection	terminates.	If	
enabled,	subsequent	connections	to	the	given	database	will	require	the	database	to	be	
reopened	and	relevant	procedure	caches	to	be	rebuilt.
*/
SELECT 
    name as [Name], 
    CASE
    WHEN is_auto_close_on = 1 THEN 'Enabled'
    WHEN is_auto_close_on = 0 THEN 'Disabled'
    END AS [Status]
    FROM sys.databases
    WHERE is_auto_close_on = 1;
    /*Rationale:
Because	authentication	of	users	for	contained	databases	occurs	within	the	database	not	at	
the	server\instance	level,	the	database	must	be	opened	every	time	to	authenticate	a	user.	
The	frequent	opening/closing	of	the	database consumes additional server resources and may contribute to a denial of service.
*/