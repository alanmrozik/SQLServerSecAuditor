/*
Description:
Korzystanie z portu innego niż domyślny (1433) pomaga chronić bazę danych przed atakami wymierzonymi w port domyślny.
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