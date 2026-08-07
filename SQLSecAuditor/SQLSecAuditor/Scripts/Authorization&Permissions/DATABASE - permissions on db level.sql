/*
Description:

*/
DECLARE @DatabaseName SYSNAME;
DECLARE @SQL NVARCHAR(MAX);

CREATE TABLE #Permissions
(
    DatabaseName    SYSNAME,
    Principal       NVARCHAR(256),
    PrincipalType   NVARCHAR(256),
    GrantedVia      NVARCHAR(256),
    PermissionState NVARCHAR(256),
    Permission      NVARCHAR(256),
    Scope           NVARCHAR(256),
    ObjectName      NVARCHAR(512)
);

DECLARE db_cursor CURSOR FAST_FORWARD FOR
SELECT name
FROM sys.databases
WHERE name not in ('master','msdb','model','tempdb','distribution')
  AND state_desc = 'ONLINE';

OPEN db_cursor;

FETCH NEXT FROM db_cursor INTO @DatabaseName;

WHILE @@FETCH_STATUS = 0
BEGIN

    SET @SQL = '
    USE ' + QUOTENAME(@DatabaseName) + ';

    ;WITH Permissions AS
    (
        SELECT
            Principal       = dp.name COLLATE DATABASE_DEFAULT,
            PrincipalType   = dp.type_desc COLLATE DATABASE_DEFAULT,
            GrantedVia      = CAST(''DIRECT'' AS nvarchar(100)) COLLATE DATABASE_DEFAULT,
            PermissionState = p.state_desc COLLATE DATABASE_DEFAULT,
            Permission      = p.permission_name COLLATE DATABASE_DEFAULT,
            Scope =
                CASE p.class_desc
                    WHEN ''DATABASE'' THEN ''DATABASE''
                    WHEN ''SCHEMA'' THEN ''SCHEMA''
                    WHEN ''OBJECT_OR_COLUMN'' THEN ''OBJECT''
                    ELSE p.class_desc
                END COLLATE DATABASE_DEFAULT,
            ObjectName =
                CASE p.class_desc
                    WHEN ''DATABASE'' THEN DB_NAME()
                    WHEN ''SCHEMA'' THEN QUOTENAME(SCHEMA_NAME(p.major_id))
                    WHEN ''OBJECT_OR_COLUMN'' THEN
                        QUOTENAME(OBJECT_SCHEMA_NAME(p.major_id))
                        + ''.''
                        + QUOTENAME(OBJECT_NAME(p.major_id))
                    ELSE CAST(p.major_id AS varchar(20))
                END COLLATE DATABASE_DEFAULT
        FROM sys.database_permissions p
        JOIN sys.database_principals dp
            ON dp.principal_id = p.grantee_principal_id
        WHERE dp.type IN (''S'',''U'',''G'',''E'',''X'')
          AND p.permission_name <> ''CONNECT''

        UNION ALL

        SELECT
            u.name COLLATE DATABASE_DEFAULT,
            u.type_desc COLLATE DATABASE_DEFAULT,
            (''ROLE: '' + r.name) COLLATE DATABASE_DEFAULT,
            p.state_desc COLLATE DATABASE_DEFAULT,
            p.permission_name COLLATE DATABASE_DEFAULT,
            CASE p.class_desc
                WHEN ''DATABASE'' THEN ''DATABASE''
                WHEN ''SCHEMA'' THEN ''SCHEMA''
                WHEN ''OBJECT_OR_COLUMN'' THEN ''OBJECT''
                ELSE p.class_desc
            END COLLATE DATABASE_DEFAULT,
            CASE p.class_desc
                WHEN ''DATABASE'' THEN DB_NAME()
                WHEN ''SCHEMA'' THEN QUOTENAME(SCHEMA_NAME(p.major_id))
                WHEN ''OBJECT_OR_COLUMN'' THEN
                    QUOTENAME(OBJECT_SCHEMA_NAME(p.major_id))
                    + ''.''
                    + QUOTENAME(OBJECT_NAME(p.major_id))
                ELSE CAST(p.major_id AS varchar(20))
            END COLLATE DATABASE_DEFAULT
        FROM sys.database_role_members rm
        JOIN sys.database_principals r
            ON rm.role_principal_id = r.principal_id
        JOIN sys.database_principals u
            ON rm.member_principal_id = u.principal_id
        JOIN sys.database_permissions p
            ON p.grantee_principal_id = r.principal_id
        WHERE u.type IN (''S'',''U'',''G'',''E'',''X'')
          AND p.permission_name <> ''CONNECT''

        UNION ALL

        SELECT
            u.name COLLATE DATABASE_DEFAULT,
            u.type_desc COLLATE DATABASE_DEFAULT,
            CAST(''ROLE MEMBERSHIP'' AS nvarchar(100)) COLLATE DATABASE_DEFAULT,
            CAST(''MEMBER'' AS nvarchar(30)) COLLATE DATABASE_DEFAULT,
            r.name COLLATE DATABASE_DEFAULT,
            CAST(''ROLE'' AS nvarchar(30)) COLLATE DATABASE_DEFAULT,
            CASE
                WHEN r.is_fixed_role = 1 THEN ''FIXED DATABASE ROLE''
                ELSE ''USER DATABASE ROLE''
            END COLLATE DATABASE_DEFAULT
        FROM sys.database_role_members rm
        JOIN sys.database_principals r
            ON rm.role_principal_id = r.principal_id
        JOIN sys.database_principals u
            ON rm.member_principal_id = u.principal_id
        WHERE u.type IN (''S'',''U'',''G'',''E'',''X'')
    )

    INSERT INTO #Permissions
    SELECT
        DB_NAME(),
        Principal,
        PrincipalType,
        GrantedVia,
        PermissionState,
        Permission,
        Scope,
        ObjectName
    FROM Permissions;
    ';

    EXEC sp_executesql @SQL;

    FETCH NEXT FROM db_cursor INTO @DatabaseName;
END

CLOSE db_cursor;
DEALLOCATE db_cursor;


SELECT
    DatabaseName,
    Principal,
    PrincipalType,
    GrantedVia,
    PermissionState,
    Permission,
    Scope,
    ObjectName
FROM #Permissions
ORDER BY
    DatabaseName,
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


DROP TABLE #Permissions;