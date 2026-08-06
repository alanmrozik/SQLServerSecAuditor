/*
Description:
The	public database	role	contains	every	user	in	the	msdb database.	SQL	Agent	proxies	
define	a	security	context	in	which	a	job	step	can	run.
Rationale:
Granting	access	to	SQL	Agent	proxies	for	the	public role	would	allow	all	users	to	utilize	the	
proxy	which	may	have	high	privileges.	This	would	likely	break	the	principle	of	least	
privileges.
*/
USE [msdb]
GO
SELECT sp.name AS proxyname
FROM dbo.sysproxylogin spl
JOIN sys.database_principals dp
ON dp.sid = spl.sid
JOIN sysproxies sp
ON sp.proxy_id = spl.proxy_id
WHERE principal_id = USER_ID('public');
GO