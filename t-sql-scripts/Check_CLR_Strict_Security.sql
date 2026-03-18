/*
Description:
The	clr strict security option	specifies	whether	the	engine	applies	the	PERMISSION_SET
on	the	assemblies.
Rationale:
Enabling use	of	CLR	assemblies	widens	the	attack	surface	of	SQL	Server	and	puts	it	at	risk	
from	both	inadvertent	and	malicious	assemblies.
*/
SELECT name,
 CAST(value as int) as value_configured,
 CAST(value_in_use as int) as value_in_use
FROM sys.configurations
WHERE name = 'clr strict security';