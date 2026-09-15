/*
Description:
This feature can be abused to conduct a denial-of-service attack against remote servers by moving query processing to the target system.
*/
SELECT 
    name as [Name], 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'remote access';
    /*
    Rationale:
Functionality	can	be	abused	to	launch	a	Denial-of-Service	(DoS)	attack	on	remote	servers	
by	off-loading	query	processing	to	a	target.*/
/*Fix:
EXECUTE sp_configure 'show advanced options', 1; 
RECONFIGURE; 
EXECUTE sp_configure 'remote access', 0; 
RECONFIGURE; 
GO 
EXECUTE sp_configure 'show advanced options', 0; 
RECONFIGURE; 
*/
