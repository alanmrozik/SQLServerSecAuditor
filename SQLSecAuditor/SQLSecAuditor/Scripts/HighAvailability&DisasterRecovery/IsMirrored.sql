SELECT
    database_id,
    DB_NAME(database_id) AS database_name,
    mirroring_state_desc,
    mirroring_role_desc,
    mirroring_partner_name
FROM sys.database_mirroring
WHERE mirroring_guid IS NOT NULL;