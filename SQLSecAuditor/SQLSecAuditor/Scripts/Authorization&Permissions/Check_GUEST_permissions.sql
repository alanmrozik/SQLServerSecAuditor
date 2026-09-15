/*
Description:
A login assumes the guest user identity when it can access SQL Server but has no user account in a database where the guest user exists.
Revoking CONNECT from guest ensures that a login cannot access database information unless permission is explicitly granted.
*/
--USE <database_name>;
--GO
IF EXISTS
(
    SELECT 1
    FROM sys.database_permissions 
    WHERE [grantee_principal_id] = DATABASE_PRINCIPAL_ID('guest') 
      AND [state_desc] LIKE 'GRANT%' 
      AND [permission_name] = 'CONNECT'
      AND DB_NAME() NOT IN ('master','tempdb','msdb')
)
BEGIN
    SELECT 
        DB_NAME() AS [Database Name], 
        'guest' AS [Database user], 
        [permission_name] AS [Permission name], 
        [state_desc] AS [Status]
    FROM sys.database_permissions 
    WHERE [grantee_principal_id] = DATABASE_PRINCIPAL_ID('guest') 
      AND [state_desc] LIKE 'GRANT%' 
      AND [permission_name] = 'CONNECT'
      AND DB_NAME() NOT IN ('master','tempdb','msdb');
END
ELSE
BEGIN
    SELECT 'The guest user has no CONNECT permission in the database' AS [Status];
END;
/*
Rationale:
A	login	assumes	the	identity	of	the	guest user	when	a	login	has	access	to	SQL	Server	but	
does	not	have	access	to	a	database	through	its	own	account	and	the	database	has	a	guest
user	account.	Revoking	the	CONNECT permission	for	the	guest user	will	ensure that a login is not able to access database information without explicit access to do so.
*/
