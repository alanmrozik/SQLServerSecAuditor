/*
Description:
Non-clustered SQL Server instances in production environments should be marked as hidden to prevent SQL Server Browser from broadcasting them.
*/
/*
Non-clustered	SQL	Server	instances	within	production	environments should	be	designated	
as	hidden	to	prevent	advertisement	by	the	SQL	Server	Browser	service.
Rationale:
Designating	production	SQL	Server	instances	as	hidden	leads	to	a	more	secure	installation	
because	they cannot be enumerated. However, clustered instances may break if this option is selected.
*/
DECLARE @getValue INT;

EXEC master.sys.xp_instance_regread
    @rootkey = N'HKEY_LOCAL_MACHINE',
    @key = N'SOFTWARE\Microsoft\Microsoft SQL Server\MSSQLServer\SuperSocketNetLib',
    @value_name = N'HideInstance',
    @value = @getValue OUTPUT;

IF @getValue IS NULL or @getValue = 0
BEGIN
    SELECT 'The instance is not hidden' AS [Status];
END
ELSE
BEGIN
    SELECT @getValue AS [HideInstance];
END;
/*Fix:
EXEC master.sys.xp_instance_regwrite 
@rootkey = N'HKEY_LOCAL_MACHINE', 
@key = N'SOFTWARE\Microsoft\Microsoft SQL 
Server\MSSQLServer\SuperSocketNetLib', 
@value_name = N'HideInstance', 
@type = N'REG_DWORD', 
@value = 1; 
*/
