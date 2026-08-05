SELECT SERVERPROPERTY('IsClustered')

SELECT
	NodeName as [Node name],
	status_description [Status],
	CASE
	WHEN is_current_owner = 1 THEN 'YES'
	WHEN is_current_owner = 0 THEN 'NO'
	END AS [Is current owner]
FROM sys.dm_os_cluster_nodes
ORDER BY nodename;

SELECT *
FROM sys.dm_os_cluster_properties;