/*
Description:
To ustawienie powoduje rejestrowanie nieudanych prób uwierzytelniania dla logowań do programu SQL Server w Errorlog. Jest to ustawienie domyślne dla programu SQL Server.
Ustawienie to było historycznie dostępne we wszystkich wersjach i edycjach programu SQL Server. Przed wprowadzeniem funkcji SQL Server Audit był to jedyny dostępny mechanizm rejestrowania logowań (zarówno udanych, jak i nieudanych).
*/
;WITH Permissions AS
(
    ------------------------------------------------------------------------
    -- Uprawnienia nadane bezpośrednio użytkownikom / grupom
    ------------------------------------------------------------------------
    SELECT
        Principal       = dp.name COLLATE DATABASE_DEFAULT,
        PrincipalType   = dp.type_desc COLLATE DATABASE_DEFAULT,
        GrantedVia      = CAST('DIRECT' AS nvarchar(100)) COLLATE DATABASE_DEFAULT,
        PermissionState = p.state_desc COLLATE DATABASE_DEFAULT,
        Permission      = p.permission_name COLLATE DATABASE_DEFAULT,
        Scope =
            (
                CASE p.class_desc
                    WHEN 'DATABASE' THEN 'DATABASE'
                    WHEN 'SCHEMA' THEN 'SCHEMA'
                    WHEN 'OBJECT_OR_COLUMN' THEN 'OBJECT'
                    ELSE p.class_desc
                END
            ) COLLATE DATABASE_DEFAULT,
        ObjectName =
            (
                CASE p.class_desc
                    WHEN 'DATABASE' THEN DB_NAME()
                    WHEN 'SCHEMA' THEN QUOTENAME(SCHEMA_NAME(p.major_id))
                    WHEN 'OBJECT_OR_COLUMN' THEN
                        QUOTENAME(OBJECT_SCHEMA_NAME(p.major_id))
                        + '.'
                        + QUOTENAME(OBJECT_NAME(p.major_id))
                    ELSE CAST(p.major_id AS varchar(20))
                END
            ) COLLATE DATABASE_DEFAULT
    FROM sys.database_permissions p
    JOIN sys.database_principals dp
        ON dp.principal_id = p.grantee_principal_id
    WHERE dp.type IN ('S','U','G','E','X')
      AND p.permission_name <> 'CONNECT'

    UNION ALL

    ------------------------------------------------------------------------
    -- Uprawnienia odziedziczone z ról
    ------------------------------------------------------------------------
    SELECT
        Principal       = u.name COLLATE DATABASE_DEFAULT,
        PrincipalType   = u.type_desc COLLATE DATABASE_DEFAULT,
        GrantedVia      = ('ROLE: ' + r.name) COLLATE DATABASE_DEFAULT,
        PermissionState = p.state_desc COLLATE DATABASE_DEFAULT,
        Permission      = p.permission_name COLLATE DATABASE_DEFAULT,
        Scope =
            (
                CASE p.class_desc
                    WHEN 'DATABASE' THEN 'DATABASE'
                    WHEN 'SCHEMA' THEN 'SCHEMA'
                    WHEN 'OBJECT_OR_COLUMN' THEN 'OBJECT'
                    ELSE p.class_desc
                END
            ) COLLATE DATABASE_DEFAULT,
        ObjectName =
            (
                CASE p.class_desc
                    WHEN 'DATABASE' THEN DB_NAME()
                    WHEN 'SCHEMA' THEN QUOTENAME(SCHEMA_NAME(p.major_id))
                    WHEN 'OBJECT_OR_COLUMN' THEN
                        QUOTENAME(OBJECT_SCHEMA_NAME(p.major_id))
                        + '.'
                        + QUOTENAME(OBJECT_NAME(p.major_id))
                    ELSE CAST(p.major_id AS varchar(20))
                END
            ) COLLATE DATABASE_DEFAULT
    FROM sys.database_role_members rm
    JOIN sys.database_principals r
        ON rm.role_principal_id = r.principal_id
    JOIN sys.database_principals u
        ON rm.member_principal_id = u.principal_id
    JOIN sys.database_permissions p
        ON p.grantee_principal_id = r.principal_id
    WHERE u.type IN ('S','U','G','E','X')
      AND p.permission_name <> 'CONNECT'

    UNION ALL

    ------------------------------------------------------------------------
    -- Członkostwo w rolach
    ------------------------------------------------------------------------
    SELECT
        Principal       = u.name COLLATE DATABASE_DEFAULT,
        PrincipalType   = u.type_desc COLLATE DATABASE_DEFAULT,
        GrantedVia      = CAST('ROLE MEMBERSHIP' AS nvarchar(100)) COLLATE DATABASE_DEFAULT,
        PermissionState = CAST('MEMBER' AS nvarchar(30)) COLLATE DATABASE_DEFAULT,
        Permission      = r.name COLLATE DATABASE_DEFAULT,
        Scope           = CAST('ROLE' AS nvarchar(30)) COLLATE DATABASE_DEFAULT,
        ObjectName      =
            (
                CASE
                    WHEN r.is_fixed_role = 1 THEN 'FIXED DATABASE ROLE'
                    ELSE 'USER DATABASE ROLE'
                END
            ) COLLATE DATABASE_DEFAULT
    FROM sys.database_role_members rm
    JOIN sys.database_principals r
        ON rm.role_principal_id = r.principal_id
    JOIN sys.database_principals u
        ON rm.member_principal_id = u.principal_id
    WHERE u.type IN ('S','U','G','E','X')
)

SELECT
    Principal,
    PrincipalType,
    GrantedVia,
    PermissionState,
    Permission,
    Scope,
    ObjectName
FROM Permissions
ORDER BY
    Principal,
    CASE PermissionState
        WHEN 'DENY' THEN 1
        WHEN 'GRANT_WITH_GRANT_OPTION' THEN 2
        WHEN 'GRANT' THEN 3
        WHEN 'MEMBER' THEN 4
        ELSE 5
    END,
    GrantedVia,
    Scope,
    ObjectName,
    Permission;