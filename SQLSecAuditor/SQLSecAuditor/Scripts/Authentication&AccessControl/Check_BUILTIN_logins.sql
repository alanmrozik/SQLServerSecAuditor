/*
Description:
Built-in groups such as Administrators, Everyone, Authenticated Users, and Guests often contain a broad membership, which conflicts with the least-privilege practice of granting SQL Server access only to users who need it.
These groups should not be used to grant any level of access to a SQL Server Database Engine instance.
*/
IF EXISTS
(
    SELECT 1
    FROM sys.server_principals pr
    JOIN sys.server_permissions pe
        ON pr.principal_id = pe.grantee_principal_id
    WHERE pr.name LIKE 'BUILTIN%'
)
BEGIN
    SELECT 
        pr.[name] AS [Name],
        pe.[permission_name] AS [Permission name],
        pe.[state_desc] AS [Status]
    FROM sys.server_principals pr
    JOIN sys.server_permissions pe
        ON pr.principal_id = pe.grantee_principal_id
    WHERE pr.name LIKE 'BUILTIN%';
END
ELSE
BEGIN
    SELECT 'No permissions are assigned to BUILTIN groups' AS [Status];
END;
/*Rationale:
The	BUILTIN groups	(Administrators,	Everyone,	Authenticated	Users,	Guests,	etc.)	generally	
contain	very	broad	memberships	which	would	not	meet	the	best	practice	of	ensuring	only	
the	necessary	users	have	been	granted	access	to	a	SQL	Server	instance.	These	groups	
should	not	be	used	for	any	level	of	access	into	a	SQL	Server	Database	Engine	instance.
*/
