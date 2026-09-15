/*
Description:
SQL Server service accounts should be domain accounts that cannot be used to log in to other services.
*/
SELECT 
    servicename as [Service name],
    service_account as [Service account name],
    status_desc as [Status],
    process_id as [Process ID] 
    FROM sys.dm_server_services
