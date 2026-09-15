/*
Description:
Disabling Database Mail XPs reduces the SQL Server attack surface and removes a denial-of-service vector and a channel for exfiltrating database data to a remote host.

*/
SELECT 
    name as [Name], 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'Database Mail XPs';
    /*Rationale:
Disabling	the	Database Mail XPs option	reduces	the	SQL	Server	surface,	eliminates	a	DOS	
attack	vector	and	channel	to	exfiltrate	data	from	the	database	server	to	a	remote host*/
/*Fix:
EXECUTE sp_configure 'show advanced options', 1; 
RECONFIGURE; 
EXECUTE sp_configure 'Database Mail XPs', 0; 
RECONFIGURE; 
GO 
EXECUTE sp_configure 'show advanced options', 0; 
RECONFIGURE; 
*/
