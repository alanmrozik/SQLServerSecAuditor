SELECT SERVERPROPERTY('IsClustered')

SELECT
	NodeName,
	status_description,
	CASE
	WHEN is_current_owner = 1 THEN 'YES'
	WHEN is_current_owner = 0 THEN 'NO'
	END AS [is_current_owner]
FROM sys.dm_os_cluster_nodes
ORDER BY nodename;

SELECT *
FROM sys.dm_os_cluster_properties;