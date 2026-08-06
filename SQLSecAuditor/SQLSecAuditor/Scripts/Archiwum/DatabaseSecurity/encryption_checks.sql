/*TDE*/
SELECT
    name,
    is_encrypted
FROM sys.databases
WHERE is_encrypted = 1;

SELECT
    DB_NAME(database_id) AS database_name,
    encryption_state,
    CASE encryption_state
        WHEN 0 THEN 'No DB encryption key'
        WHEN 1 THEN 'Unencrypted'
        WHEN 2 THEN 'Encryption in progress'
        WHEN 3 THEN 'Encrypted'
        WHEN 4 THEN 'Key change in progress'
        WHEN 5 THEN 'Decryption in progress'
    END AS encryption_state_desc,
    percent_complete
FROM sys.dm_database_encryption_keys;

/*Always Encrypted*/
SELECT *
FROM sys.column_master_keys;

SELECT *
FROM sys.column_encryption_keys;

SELECT
    t.name AS table_name,
    c.name AS column_name,
    c.encryption_type_desc,
    c.encryption_algorithm_name
FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
WHERE c.encryption_type IS NOT NULL;

/*certyfikaty i klucze*/
SELECT
    name,
    subject,
    expiry_date,
    start_date
FROM sys.certificates;

SELECT
    name,
    key_length,
    algorithm_desc,
    key_guid
FROM sys.symmetric_keys;

SELECT
    name,
    algorithm_desc,
    key_length
FROM sys.asymmetric_keys;

/*wszystko*/
SELECT
    'TDE' AS feature,
    CASE WHEN EXISTS (SELECT 1 FROM sys.dm_database_encryption_keys)
         THEN 'POSSIBLE IN USE' ELSE 'NOT USED' END AS status

UNION ALL

SELECT
    'Always Encrypted',
    CASE WHEN EXISTS (SELECT 1 FROM sys.column_master_keys)
         THEN 'USED' ELSE 'NOT USED' END

UNION ALL

SELECT
    'Certificates',
    CASE WHEN EXISTS (SELECT 1 FROM sys.certificates)
         THEN 'PRESENT' ELSE 'NONE' END

UNION ALL

SELECT
    'Symmetric Keys',
    CASE WHEN EXISTS (SELECT 1 FROM sys.symmetric_keys)
         THEN 'PRESENT' ELSE 'NONE' END;