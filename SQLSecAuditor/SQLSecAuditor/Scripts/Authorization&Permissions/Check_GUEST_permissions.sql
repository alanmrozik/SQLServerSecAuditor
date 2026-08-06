/*
Description:
Login przyjmuje tożsamość użytkownika typu „guest” (gość), gdy ma dostęp do serwera SQL, ale nie posiada dostępu do konkretnej bazy danych za pośrednictwem własnego konta, a baza ta zawiera konto użytkownika typu „guest”. 
Odebranie uprawnienia CONNECT użytkownikowi typu „guest” gwarantuje, że login nie uzyska dostępu do informacji w bazie danych bez wyraźnego nadania takich uprawnień.
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
    SELECT 'Użytkownik guest nie posiada nadanych uprawnień CONNECT w bazie danych' AS [Status];
END;
/*
Rationale:
A	login	assumes	the	identity	of	the	guest user	when	a	login	has	access	to	SQL	Server	but	
does	not	have	access	to	a	database	through	its	own	account	and	the	database	has	a	guest
user	account.	Revoking	the	CONNECT permission	for	the	guest user	will	ensure that a login is not able to access database information without explicit access to do so.
*/