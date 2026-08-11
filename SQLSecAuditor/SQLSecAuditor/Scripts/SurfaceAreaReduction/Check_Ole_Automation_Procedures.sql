/*
Description:
Włączenie tej opcji zwiększy powierzchnię ataku na serwer SQL i umożliwi użytkownikom wykonywanie funkcji w kontekście zabezpieczeń serwera SQL.
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
/*Fix:
EXECUTE sp_configure 'show advanced options', 1; 
RECONFIGURE; 
EXECUTE sp_configure 'Ole Automation Procedures', 0; 
RECONFIGURE; 
GO 
EXECUTE sp_configure 'show advanced options', 0; 
RECONFIGURE;  
*/