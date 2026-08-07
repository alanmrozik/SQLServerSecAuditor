/*
Description:
Enabling	Ad	Hoc	Distributed	Queries	allows	users	to	query	data	and	execute	statements	on	
external	data	sources.	This	functionality	should	be	disabled.
Rationale:
This	feature	can	be	used	to	remotely	access	and	exploit	vulnerabilities	on	remote	SQL	
Server	instances	and	to	run	unsafe	Visual	Basic	for	Application	functions.
*/
SELECT 
    name, 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'Ad Hoc Distributed Queries';