SELECT 
	rp.name,
    sp.name AS login_name,
    sp.type_desc AS login_type,
    sp.is_disabled,
    sp.create_date,
    sp.modify_date
FROM sys.server_role_members rm
JOIN sys.server_principals sp 
    ON rm.member_principal_id = sp.principal_id
JOIN sys.server_principals rp 
    ON rm.role_principal_id = rp.principal_id
WHERE rp.name IN ('sysadmin','securityadmin','serveradmin','setupadmin')
ORDER BY sp.name;