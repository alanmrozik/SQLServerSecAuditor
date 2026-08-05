/*
Description:
Remove	the	right	of	the	guest user	to	connect	to	SQL	Server	databases,	except	for	master,	
msdb,	and	tempdb.
Rationale:
A	login	assumes	the	identity	of	the	guest user	when	a	login	has	access	to	SQL	Server	but	
does	not	have	access	to	a	database	through	its	own	account	and	the	database	has	a	guest
user	account.	Revoking	the	CONNECT permission	for	the	guest user	will	ensure that a login is not able to access database information without explicit access to do so.
*/
USE <database_name>;
GO
SELECT DB_NAME() AS [Database Name], 'guest' AS [Database user], 
[permission_name] AS [Permission name], [state_desc] as [Status]
FROM sys.database_permissions 
WHERE [grantee_principal_id] = DATABASE_PRINCIPAL_ID('guest') 
AND [state_desc] LIKE 'GRANT%' 
AND [permission_name] = 'CONNECT'
AND DB_NAME() NOT IN ('master','tempdb','msdb');