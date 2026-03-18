/*
Description:
The	default	trace	provides	audit	logging	of	database	activity	including	account	creations,	
privilege	elevation	and	execution	of	DBCC	commands.
Rationale:
Default	trace	provides	valuable	audit	information	regarding	security-related	activities	on	
the	server.
*/
SELECT name,
 CAST(value as int) as value_configured,
 CAST(value_in_use as int) as value_in_use
FROM sys.configurations
WHERE name = 'default trace enabled';