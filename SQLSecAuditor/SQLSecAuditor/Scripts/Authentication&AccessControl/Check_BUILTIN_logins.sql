/*
Description:
Grupy wbudowane (BUILTIN), takie jak Administrators, Everyone, Authenticated Users, Guests itp., zazwyczaj obejmują bardzo szeroki krąg członków, co jest niezgodne z zasadą najlepszych praktyk, zakładającą przyznawanie dostępu do instancji programu SQL Server wyłącznie niezbędnym użytkownikom. 
Grup tych nie należy wykorzystywać do nadawania jakiegokolwiek poziomu dostępu do instancji aparatu bazy danych SQL Server.
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
    SELECT 'Brak uprawnień przypisanych do grup wbudowanych BUILTIN' AS [Status];
END;
/*Rationale:
The	BUILTIN groups	(Administrators,	Everyone,	Authenticated	Users,	Guests,	etc.)	generally	
contain	very	broad	memberships	which	would	not	meet	the	best	practice	of	ensuring	only	
the	necessary	users	have	been	granted	access	to	a	SQL	Server	instance.	These	groups	
should	not	be	used	for	any	level	of	access	into	a	SQL	Server	Database	Engine	instance.
*/