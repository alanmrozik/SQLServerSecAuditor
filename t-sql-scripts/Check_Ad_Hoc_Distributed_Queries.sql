/*
Description:
Enabling	Ad	Hoc	Distributed	Queries	allows	users	to	query	data	and	execute	statements	on	
external	data	sources.	This	functionality	should	be	disabled.
Rationale:
This	feature	can	be	used	to	remotely	access	and	exploit	vulnerabilities	on	remote	SQL	
Server	instances	and	to	run	unsafe	Visual	Basic	for	Application	functions.
*/
SELECT name, CAST(value as int) as value_configured, CAST(value_in_use as 
int) as value_in_use 
FROM sys.configurations 
WHERE name = 'Ad Hoc Distributed Queries';