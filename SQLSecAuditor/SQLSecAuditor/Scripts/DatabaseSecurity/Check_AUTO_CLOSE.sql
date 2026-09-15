/*
Description:
AUTO_CLOSE determines whether a database closes after the last connection ends.
When enabled, subsequent connections must reopen the database and rebuild the relevant procedure caches.
*/
IF EXISTS
(
    SELECT 1
    FROM sys.databases
    WHERE is_auto_close_on = 1
)
BEGIN
    SELECT 
        name AS [Name], 
        CASE
            WHEN is_auto_close_on = 1 THEN 'Enabled'
            WHEN is_auto_close_on = 0 THEN 'Disabled'
        END AS [Status]
    FROM sys.databases
    WHERE is_auto_close_on = 1;
END
ELSE
BEGIN
    SELECT 'AUTO_CLOSE is disabled for all databases' AS [Status];
END;
    /*Rationale:
Because	authentication	of	users	for	contained	databases	occurs	within	the	database	not	at	
the	server\instance	level,	the	database	must	be	opened	every	time	to	authenticate	a	user.	
The	frequent	opening/closing	of	the	database consumes additional server resources and may contribute to a denial of service.
*/
/*Fix:
SELECT * FROM test;
*/
/*Fix:
ALTER DATABASE <database_name> SET AUTO_CLOSE OFF; 
*/
