SELECT 
    name,
    is_disabled
FROM sys.server_principals
WHERE name = 'sa';