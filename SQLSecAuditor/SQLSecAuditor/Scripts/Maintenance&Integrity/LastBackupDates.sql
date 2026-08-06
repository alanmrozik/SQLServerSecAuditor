/*
Description:
Warto jest mieć świeże backupy.
*/
;WITH backup_cte AS
(
    SELECT
        database_name,
        backup_type =
            CASE type
                WHEN 'D' THEN 'Full'
                WHEN 'I' THEN 'Differential'
                WHEN 'L' THEN 'Log'
                ELSE type
            END,
        backup_start_date,
        backup_finish_date,
        ROW_NUMBER() OVER
        (
            PARTITION BY database_name, type
            ORDER BY backup_finish_date DESC
        ) AS rownum
    FROM msdb.dbo.backupset
)
SELECT
    d.name AS DatabaseName,
    MAX(CASE WHEN b.backup_type = 'Full'
        THEN b.backup_finish_date END) AS LastFullBackup,

    MAX(CASE WHEN b.backup_type = 'Differential'
        THEN b.backup_finish_date END) AS LastDifferentialBackup,

    MAX(CASE WHEN b.backup_type = 'Log'
        THEN b.backup_finish_date END) AS LastLogBackup

FROM sys.databases d
LEFT JOIN backup_cte b
    ON d.name = b.database_name
    AND b.rownum = 1

WHERE d.state_desc = 'ONLINE'

GROUP BY d.name
ORDER BY d.name;