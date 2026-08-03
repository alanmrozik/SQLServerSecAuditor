/*
Description:
If	installed,	a	default	SQL	Server	instance	will	be	assigned	a	default	port	of	TCP:1433 for	
TCP/IP	communication.	Administrators	can	also	manually	configure	named	instances	to	
use	TCP:1433 for	communication.	TCP:1433 is	a	widely	known	SQL	Server	port	and	this	
port	assignment	should	be	changed.	In	a	multi-instance	scenario,	each	instance	must	be	
assigned	its	own	dedicated	TCP/IP	port.
Rationale:
Using	a	non-default	port	helps	protect	the	database	from	attacks	directed	to	the	default	
port.
*/
SELECT TOP(1) local_tcp_port FROM sys.dm_exec_connections
WHERE local_tcp_port IS NOT NULL;
/* or */
SELECT local_tcp_port
FROM sys.dm_exec_connections
WHERE session_id = @@SPID