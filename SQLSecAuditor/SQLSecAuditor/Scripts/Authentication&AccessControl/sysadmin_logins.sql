/*
Description:
Liczba użytkowników przypisanych do roli sysadmin powinna być minimalna, ograniczona do administratora serwera lub baz danych. Pozostali użytkownicy powinni zostać zweryfikowani.
*/
SELECT 
	rp.name as [Role name],
    sp.name AS [Login name],
    sp.type_desc AS [Login type],
    sp.is_disabled AS [Is disabled],
    sp.create_date AS [Creation date],
    sp.modify_date AS [Modification date]
FROM sys.server_role_members rm
JOIN sys.server_principals sp 
    ON rm.member_principal_id = sp.principal_id
JOIN sys.server_principals rp 
    ON rm.role_principal_id = rp.principal_id
WHERE rp.name IN ('sysadmin','securityadmin','serveradmin','setupadmin')
ORDER BY sp.name;