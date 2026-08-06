/*
Description:
The	clr strict security option	specifies	whether	the	engine	applies	the	PERMISSION_SET
on	the	assemblies.
*/
SELECT 
    name as [Name], 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'clr strict security';
    /*Rationale:
Enabling use	of	CLR	assemblies	widens	the	attack	surface	of	SQL	Server	and	puts	it	at	risk	
from	both	inadvertent	and	malicious	assemblies.*/