/*
Description:
Using a port other than the default port 1433 helps protect the database from attacks targeting the default port.
*/
/*
Rationale:
Using	a	non-default	port	helps	protect	the	database	from	attacks	directed	to	the	default	
port.
SELECT TOP(1) local_tcp_port as [Local TCP port] FROM sys.dm_exec_connections
WHERE local_tcp_port IS NOT NULL;
*/ 

SELECT local_tcp_port
FROM sys.dm_exec_connections
WHERE session_id = @@SPID
