/*
Description:
The	clr enabled option	specifies	whether	user	assemblies	can	be	run	by	SQL	Server.
Rationale:
Enabling	use	of	CLR	assemblies	widens	the	attack	surface	of	SQL	Server	and	puts	it	at	risk	
from	both	inadvertent	and	malicious	assemblies.
*/
--USE [<database_name>]
--GO
IF EXISTS (
    SELECT 1
    FROM sys.assemblies
    WHERE is_user_defined = 1
)
BEGIN
    SELECT
        name AS [Assembly name],
        permission_set_desc AS [Permission set]
    FROM sys.assemblies
    WHERE is_user_defined = 1;
END
ELSE
BEGIN
    SELECT 'CLR nie jest włączony' AS [Status];
END;