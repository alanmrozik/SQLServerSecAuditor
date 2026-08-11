/*
Description:
Dedykowane połączenie administratora (DAC) umożliwia administratorowi uzyskanie dostępu do działającego serwera w celu wykonania funkcji diagnostycznych lub instrukcji języka Transact-SQL, bądź też rozwiązania problemów z serwerem – nawet jeśli jest on zablokowany, działa w nietypowym stanie lub nie odpowiada na standardowe próby nawiązania połączenia z aparatem bazy danych SQL Server. 
*/
USE master;
GO
SELECT 
    name as [Name], 
    CASE
    WHEN value_in_use = 0 THEN 'Disabled'
    WHEN value_in_use = 1 THEN 'Enabled'
    END AS [Status]
    FROM sys.configurations 
    WHERE name = 'remote admin connections'
    AND SERVERPROPERTY('IsClustered') = 0;
    /*
    Rationale:
The	Dedicated	Administrator	Connection	(DAC)	lets	an	administrator	access	a	running	
server	to	execute	diagnostic	functions	or	Transact-SQL	statements,	or	to	troubleshoot	
problems	on	the	server,	even	when	the	server	is	locked	or	running	in	an	abnormal	state	
and	not	responding	to	a	SQL	Server	Database	Engine	connection.	In	a	cluster	scenario,	the	
administrator	may	not	actually	be	logged	on	to	the	same	node	that	is	currently	hosting	the	
SQL	Server	instance	and	thus	is	considered	"remote".	Therefore,	this	setting	should	usually	
be	enabled	(1) for SQL Server failover clusters;otherwise, it should be disabled (0) which is the default.*/
/*Fix:
EXECUTE sp_configure 'remote admin connections', 0; 
RECONFIGURE; 
GO 
*/