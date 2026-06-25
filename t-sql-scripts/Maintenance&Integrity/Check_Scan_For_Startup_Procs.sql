/*
Description:
The	scan for startup procs option,	if	enabled,	causes	SQL	Server	to	scan	for	and	
automatically	run	all	stored procedures	that	are	set	to	execute	upon	service	startup.
Rationale:
Enforcing	this	control	reduces	the	threat	of	an	entity	leveraging	these	facilities	for	
malicious	purposes.
*/
SELECT 
    name, 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'scan for startup procs';