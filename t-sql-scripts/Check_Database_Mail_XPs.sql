/*
Description:
The	Database Mail XPs option	controls	the	ability	to	generate	and	transmit	email	
messages	from	SQL	Server.
Rationale:
Disabling	the	Database Mail XPs option	reduces	the	SQL	Server	surface,	eliminates	a	DOS	
attack	vector	and	channel	to	exfiltrate	data	from	the	database	server	to	a	remote host
*/
SELECT name,
 CAST(value as int) as value_configured,
 CAST(value_in_use as int) as value_in_use
FROM sys.configurations
WHERE name = 'Database Mail XPs';