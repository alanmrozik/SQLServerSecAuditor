/*
Description:
The	remote admin connections option	controls	whether	a	client	application	on	a	remote	
computer	can	use	the	Dedicated	Administrator	Connection	(DAC).
Rationale:
The	Dedicated	Administrator	Connection	(DAC)	lets	an	administrator	access	a	running	
server	to	execute	diagnostic	functions	or	Transact-SQL	statements,	or	to	troubleshoot	
problems	on	the	server,	even	when	the	server	is	locked	or	running	in	an	abnormal	state	
and	not	responding	to	a	SQL	Server	Database	Engine	connection.	In	a	cluster	scenario,	the	
administrator	may	not	actually	be	logged	on	to	the	same	node	that	is	currently	hosting	the	
SQL	Server	instance	and	thus	is	considered	"remote".	Therefore,	this	setting	should	usually	
be	enabled	(1) for SQL Server failover clusters;otherwise, it should be disabled (0) which is the default.
*/
USE master;
GO
SELECT name, 
CAST(value as int) as value_configured,
CAST(value_in_use as int) as value_in_use
FROM sys.configurations
WHERE name = 'remote admin connections'
AND SERVERPROPERTY('IsClustered') = 0;