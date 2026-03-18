/*
Description:
The	remote access option	controls	the	execution	of	local	stored	procedures	on	remote	
servers	or	remote	stored	procedures	on	local	server.
Rationale:
Functionality	can	be	abused	to	launch	a	Denial-of-Service	(DoS)	attack	on	remote	servers	
by	off-loading	query	processing	to	a	target.
*/
SELECT name,
 CAST(value as int) as value_configured,
 CAST(value_in_use as int) as value_in_use
FROM sys.configurations
WHERE name = 'remote access';