/*
Description:
The	scan for startup procs option,	if	enabled,	causes	SQL	Server	to	scan	for	and	
automatically	run	all	stored procedures	that	are	set	to	execute	upon	service	startup.
Rationale:
Enforcing	this	control	reduces	the	threat	of	an	entity	leveraging	these	facilities	for	
malicious	purposes.
*/
SELECT name,
 CAST(value as int) as value_configured,
 CAST(value_in_use as int) as value_in_use
FROM sys.configurations
WHERE name = 'scan for startup procs';