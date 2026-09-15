/*
Description:
This feature can be used to access and exploit vulnerabilities in remote SQL Server instances and to run unsafe Visual Basic for Applications functions.
*/
SELECT 
    name, 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'Ad Hoc Distributed Queries';
    /*
    Rationale:
This	feature	can	be	used	to	remotely	access	and	exploit	vulnerabilities	on	remote	SQL	
Server	instances	and	to	run	unsafe	Visual	Basic	for	Application	functions.
*/
/*Fix:
EXECUTE sp_configure 'show advanced options', 1; 
RECONFIGURE; 
EXECUTE sp_configure 'Ad Hoc Distributed Queries', 0; 
RECONFIGURE; 
GO 
EXECUTE sp_configure 'show advanced options', 0; 
RECONFIGURE; 
*/
