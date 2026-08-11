/*
Description:
Konto „sa” to powszechnie znane i często używane konto w programie SQL Server, posiadające uprawnienia administratora systemu (sysadmin). 
Jest to domyślny login utworzony podczas instalacji, który zawsze ma przypisane wartości principal_id=1 oraz sid=0x01.
*/
SELECT 
    name as [Name], 
    CASE 
    WHEN is_disabled = 1 THEN 'Disabled'
    WHEN is_disabled = 0 THEN 'Enabled'
    END as [Status]
    FROM sys.server_principals
    WHERE sid = 0x01;
    /*
    Rationale:
Enforcing	this	control	reduces	the	probability	of	an	attacker	executing	brute	force	attacks	
against	a	well-known	principal.
*/
/*Fix:
USE [master] 
GO 
DECLARE @tsql nvarchar(max) 
SET @tsql = 'ALTER LOGIN ' + SUSER_NAME(0x01) + ' DISABLE' 
EXEC (@tsql) 
GO 
*/