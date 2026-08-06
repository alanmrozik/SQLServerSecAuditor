/*
Description:
Przyznanie roli public dostępu do serwerów proxy agenta SQL umożliwiłoby wszystkim użytkownikom korzystanie z proxy, które może dysponować wysokimi uprawnieniami. Prawdopodobnie naruszyłoby to zasadę minimalnych uprawnień.
*/
USE [msdb]
GO
IF EXISTS
(
    SELECT 1
    FROM dbo.sysproxylogin spl
    JOIN sys.database_principals dp
        ON dp.sid = spl.sid
    JOIN sysproxies sp
        ON sp.proxy_id = spl.proxy_id
    WHERE principal_id = USER_ID('public')
)
BEGIN
    SELECT 
        sp.name AS proxyname
    FROM dbo.sysproxylogin spl
    JOIN sys.database_principals dp
        ON dp.sid = spl.sid
    JOIN sysproxies sp
        ON sp.proxy_id = spl.proxy_id
    WHERE principal_id = USER_ID('public');
END
ELSE
BEGIN
    SELECT 'Użytkownik public nie posiada dostępu do proxy SQL Server Agent' AS [Status];
END;
/*Rationale:
Granting	access	to	SQL	Agent	proxies	for	the	public role	would	allow	all	users	to	utilize	the	
proxy	which	may	have	high	privileges.	This	would	likely	break	the	principle	of	least	
privileges.
*/