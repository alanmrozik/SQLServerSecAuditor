/*
Description:
The	clr enabled option	specifies	whether	user	assemblies	can	be	run	by	SQL	Server.
Rationale:
Enabling	use	of	CLR	assemblies	widens	the	attack	surface	of	SQL	Server	and	puts	it	at	risk	
from	both	inadvertent	and	malicious	assemblies.

PER EVERY DB
*/
SELECT name AS Assembly_Name, permission_set_desc
FROM sys.assemblies
WHERE is_user_defined = 1;