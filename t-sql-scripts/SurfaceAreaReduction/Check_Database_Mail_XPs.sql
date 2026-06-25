/*
Description:
The	Database Mail XPs option	controls	the	ability	to	generate	and	transmit	email	
messages	from	SQL	Server.
Rationale:
Disabling	the	Database Mail XPs option	reduces	the	SQL	Server	surface,	eliminates	a	DOS	
attack	vector	and	channel	to	exfiltrate	data	from	the	database	server	to	a	remote host
*/
SELECT 
    name, 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'Database Mail XPs';