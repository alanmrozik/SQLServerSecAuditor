IF CAST(SERVERPROPERTY('IsHadrEnabled') AS INT) = 1
BEGIN
    SELECT
        ag.name AS [Name],
        ar.replica_server_name AS [Replicas name],
        ars.role_desc AS [Role],
        ars.connected_state_desc AS [Connection status],
        ars.synchronization_health_desc AS [Synchronization status]
    FROM sys.availability_groups ag
    INNER JOIN sys.availability_replicas ar
        ON ag.group_id = ar.group_id
    INNER JOIN sys.dm_hadr_availability_replica_states ars
        ON ar.replica_id = ars.replica_id;
END
ELSE
BEGIN
    SELECT
        'brak ag' AS [Status];
END;