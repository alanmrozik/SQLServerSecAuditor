IF EXISTS (
    SELECT 1
    FROM sys.database_mirroring
    WHERE mirroring_guid IS NOT NULL
)
BEGIN
    SELECT
        DB_NAME(database_id) AS [Database name],
        mirroring_state_desc AS [Status],
        mirroring_role_desc AS [Role],
        mirroring_partner_name AS [Partner name]
    FROM sys.database_mirroring
    WHERE mirroring_guid IS NOT NULL;
END
ELSE
BEGIN
    SELECT 'Brak database mirroring' AS [Status];
END;