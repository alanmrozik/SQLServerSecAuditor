/*
Description:
The	default	trace	provides	audit	logging	of	database	activity	including	account	creations,	
privilege	elevation	and	execution	of	DBCC	commands.
Rationale:
Default	trace	provides	valuable	audit	information	regarding	security-related	activities	on	
the	server.
*/
SELECT 
    name, 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'default trace enabled';